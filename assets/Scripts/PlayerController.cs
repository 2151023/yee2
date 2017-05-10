using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider[] Wheels; //

    public bool Motor; // is this wheel attached to the motor?
    public bool Steering; // does this wheel apply steer angle?
    public bool Braking; // is this wheel able to break?
}

[SerializeField]
public enum WeaponType
{
    Beam,
    Projectile,
}

[System.Serializable]
public class Weapon
{
    public WeaponType Type;
    [ColorUsage(true)] public Color Color;
    [Range(1, 100)] public int Damage = 1;
    [Tooltip("Fire animation speed multiplier.")] [Range(0.3f, 10f)] public float FireRate;

    public bool Enabled = true;
    //[Tooltip("Time in seconds between shots.")] public float Cooldown;
}


public class PlayerController : NetworkBehaviour
{
    private HUDController _hud;
    [SerializeField] private HealthBarController _healthBar;

    [Header("Vehicle Options")] public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    [RangeAttribute(15f, 45f)] public float maxSteeringAngle; // maximum steer angle the wheel can have
    public float maxBrakingTorque; // maximum torque the brakes apply to the wheel

    private float _braking = 0f;
    private float _steering = 0f;
    private float _motor = 0f;

    [Space(10f), SerializeField, Range(1f, 200f)] private int _maxHitpoints = 5;

    [Header("Turret Options")] [SerializeField] private Transform _rotationPoint;
    [SerializeField] private Transform _exitPoint;

    [SerializeField] private bool _lockRotationOnX = true;
    [SerializeField] private float _projectileSpeed = 50f;
    [SerializeField] private List<Weapon> _weapons;
    [SerializeField] private GameObject _flarePrefab;

    [SyncVar(hook = "OnChangeHealth")] private int _hitPoints;
    [SyncVar(hook = "OnChangeSelectedWeapon")] public int SelectedWeapon;

    [SyncVar(hook = "OnScoreChanged")] private int _score = 0;

    private GameObject _flareInstance;
    private bool _canShoot;
    private float _beamMaxDistance;
    private Animator _animator;
    private LineRenderer _lineRenderer;
    private Transform _projectilePool;
    private RaycastHit _hit;
    private int _newWeapon;
    private float _machineGunDisableTime;

    // Resolve game object references before other OnStart*() are called
    public override void PreStartClient()
    {
        Debug.Log("PreStartClient()" + gameObject.GetInstanceID());
    }

    // Includes the local client on the host.
    public override void OnStartClient()
    {
        Debug.Log("OnStartClient" + gameObject.GetInstanceID());
    }

    // Called after OnStartClient(). Good for activating cameras and input.
    public override void OnStartLocalPlayer()
    {
        Debug.Log("OnStartLocalPlayer()" + gameObject.GetInstanceID());

        Camera.main.GetComponent<FollowTarget>().target = transform;
        Camera.main.transform.LookAt(transform);
        transform.GetChild(0).GetComponent<MeshRenderer>().material.color = Color.red; // TEST
    }

    // Is not called on remote clients.
    public override void OnStartServer()
    {
        Debug.Log("OnStartServer()" + gameObject.GetInstanceID());

        _hitPoints = _maxHitpoints;


        _flareInstance = Instantiate(_flarePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        if (_flareInstance != null) _flareInstance.GetComponent<ParticleSystem>().Stop();
        NetworkServer.Spawn(_flareInstance);
    }

    private void Start()
    {
        Debug.Log("Start()" + gameObject.GetInstanceID());
        _hud = FindObjectOfType<HUDController>();

        _animator = GetComponent<Animator>();

        SelectedWeapon = 0;
        _newWeapon = SelectedWeapon;
        _canShoot = true;

        _beamMaxDistance = 80f;
        _lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Handle input
        _motor = maxMotorTorque * MInput.Vertical();
        _steering = maxSteeringAngle * MInput.Horizontal();
        _braking = maxBrakingTorque * MInput.Brake();

        // Update HUD
        _hud.UpdateSpeed(GetSpeedKph());

        float mgTimer = _machineGunDisableTime - Time.time;
        if (mgTimer >= 0)
        {
            _hud.UpdateMachineGunTimer(mgTimer);
        }
    }

    private void FixedUpdate()
    {

        // Apply input
        foreach (AxleInfo axleInfo in axleInfos)
        {
            foreach (WheelCollider wheel in axleInfo.Wheels)
            {
                if (axleInfo.Steering)
                {
                    wheel.steerAngle = _steering;
                }
                if (axleInfo.Motor)
                {
                    wheel.motorTorque = _motor;
                }
                if (axleInfo.Braking)
                {
                    wheel.brakeTorque = _braking;
                }

                // Apply rotation to visual wheel
                ApplyLocalPositionToVisuals(wheel);
            }
        }

        if (!isLocalPlayer) // TODO: refactor this
        {
            return;
        }

        //TODO: Move input to Update(), apply it here
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Debug.DrawRay(ray.origin, ray.direction, Color.yellow, Time.fixedDeltaTime); // DEBUG

        // Rotate the turret to where the user has the mouse cursor or right stick
        if (Physics.Raycast(ray.origin, ray.direction, out _hit /*, _beamMaxDistance) /* TODO: layer mask, ignore car*/))
        {
            Vector3 direction = _hit.point - _exitPoint.position;
            //Vector3 direction = _hit.point - _rotationPoint.position;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            const float f = 5f;
            Vector3 rotation = Quaternion.Lerp(_rotationPoint.rotation, lookRotation, Time.deltaTime * f).eulerAngles;

            // Cap rotation over the XX axis between 10 and 60 degrees if not locked
            float xRotation = _lockRotationOnX ? 0f : Mathf.Clamp(rotation.x, -60f, 10f);

            // Apply rotation to the turret
            _rotationPoint.rotation = Quaternion.Euler(xRotation, rotation.y, 0f);
        }

        // Handle weapon selection and firing
        if (_canShoot)
        {
            // TODO: only select new weapon if it is enabled
            _newWeapon = MInput.SelectWeapon(SelectedWeapon);
            if (_newWeapon != SelectedWeapon)
            {
                SelectedWeapon = _newWeapon;
                AnimSpin();

                _hud.UpdateSelectedWeapon(SelectedWeapon);
            }

            if (MInput.Fire())
            {
                CmdFire(_weapons[SelectedWeapon]);
            }
        }
    }

    private void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider == null || collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    public float GetSpeedKph()
    {
        return GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
    }

    // Change the selected weapon
    public void ChangeWeapon(int newWeapon)
    {
        // Only proceed if not already firing and new weapon is different
        if (!_canShoot || newWeapon == SelectedWeapon) return;

        AnimSpin();

        // Change weapon and notify game manager
        SelectedWeapon = newWeapon % _weapons.Count;

        _hud.UpdateSelectedWeapon(SelectedWeapon);
    }

    [Server]
    public void TakeDamage(int damage)
    {
        _hitPoints -= damage;
        if (_hitPoints <= 0)
        {
            Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
            RpcRespawn(spawnPoint.position, spawnPoint.rotation);
            _hitPoints = _maxHitpoints;
        }
    }

    /*
        SyncVar hooks
            - Runs on the client (including the local client)
            - Invoked by the server automatically, when the value changes
            - These are applied before OnStartClient().
     */
    public void OnChangeHealth(int health)
    {
        Debug.Log("Health: " + health + " Healthbar: " + _healthBar);
        _healthBar.SetHealth(health, _maxHitpoints);
    }

    public void OnChangeSelectedWeapon(int weaponIndex)
    {
        AnimSpin();

        _hud.UpdateSelectedWeapon(SelectedWeapon);
    }

    public void OnScoreChanged(int score)
    {
        //_hud.SetScore(this.netId, score);
    }
    // SyncVars

    /*
        RPC
            - Runs on the client
            - Invoked by the server manually.
    */

    [ClientRpc]
    private void RpcRespawn(Vector3 spawnPos, Quaternion spawnRot)
    {
        if (isLocalPlayer)
        {
            transform.position = spawnPos;
            transform.rotation = spawnRot;
        }
    }

    // RPC

    public void AddHealth(int amount)
    {
        // Only works on server
        if (!isServer)
        {
            return;
        }

        _hitPoints = Mathf.Clamp(_hitPoints + amount, 0, _maxHitpoints);
    }

    // Fire the selected weapon
    [Command]
    public void CmdFire(Weapon weapon)
    {
        // TODO: remove enabled check once we disallow even changing to a weapon if it's disabled?
        if (!_canShoot || !weapon.Enabled) return;

        if (weapon.Type == WeaponType.Beam)
        {
            _lineRenderer.SetPosition(1, Vector3.forward * _beamMaxDistance);
        }

        _canShoot = false;
        _animator.SetFloat("fireRate", weapon.FireRate);
        _animator.SetTrigger("fire");
    }

    /*   Animation methods   */

    // Fire animation begins
    private void AnimFireBegin()
    {
        var weapon = _weapons[SelectedWeapon];

        switch (weapon.Type)
        {
            case WeaponType.Projectile:
                // Get a projectile from the pool and fire it
                GameManager.Instance.FireProjectile(gameObject, weapon.Damage, _exitPoint, _projectileSpeed, weapon.Color);
                break;
            case WeaponType.Beam:
                _lineRenderer.SetColors(weapon.Color, weapon.Color);
                if (Physics.Raycast(_exitPoint.position, _exitPoint.forward, out _hit, _beamMaxDistance))
                {
                    // If we hit a target set the beam length to the distance to the hitpoint
                    _lineRenderer.SetPosition(1, Vector3.forward * _hit.distance);

                    _flareInstance.transform.position = _hit.point;
                    if (!_flareInstance.GetComponent<ParticleSystem>().isPlaying)
                    {
                        _flareInstance.GetComponent<ParticleSystem>().Play();
                    }
                }
                else
                {
                    // If we didn't hit anything set the beam length to its max distance
                    _lineRenderer.SetPosition(1, Vector3.forward * _beamMaxDistance);
                }

                // Play the beam animation
                _animator.SetTrigger("fireBeam");
                break;
            default:
                Debug.Log("Tried firing unknown weapon.");
                break;
        }
    }


    // Fire animation ended
    private void AnimFireEnd()
    {
        _lineRenderer.SetPosition(1, Vector3.zero);
        _canShoot = true;
    }

    // Stop the flare particle. Is called from the animator.
    private void AnimKillParticles()
    {
        _flareInstance.GetComponent<ParticleSystem>().Stop();
    }

    // Plays the spinning animation. To be used when changing weapons.
    private void AnimSpin()
    {
        _canShoot = false;
        _animator.SetTrigger("spin");
    }

    // Turret finished spinning animation.
    private void AnimSpinEnd()
    {
        _canShoot = true;

        // TODO: Play sound
    }

    public void EnableMachineGun()
    {
        _weapons[1].Enabled = true;
        _machineGunDisableTime = Time.time + 20f;
        Invoke("DisableMachineGun", 20f);
    }

    public void DisableMachineGun()
    {
        _weapons[1].Enabled = false;
    }
}