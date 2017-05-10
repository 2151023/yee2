using UnityEngine;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    // Part of the singleton pattern
    private static GameManager _instance = null;

    public static GameManager Instance
    {
        get { return _instance; }
    }

    private Transform _projectilePool, _explosionPool;
    private NetworkStartPosition[] _spawnPoints;

    [Header("Prefabs")] [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private GameObject _HUDPrefab;

    [Header("Powerups")] [SerializeField, Range(0f, 120f)] private float _healthCooldown = 15.0f;
    [SerializeField, Range(0f, 120f)] private float _minigunCooldown = 40f;
    [Space(5), SerializeField, Range(1, 200)] private int _healthValue = 3;
    [Space(5), SerializeField, Range(0f, 200f), Tooltip("Distance in meters from the center of the scene.")] private float _powerupSpawnRadius = 70f;

    // private PlayerController _vehicleController;


    // Awake is always called before Start()
    private void Awake()
    {
        // Singleton pattern
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);

        // Setup references between scripts
        // _vehicleController = FindObjectOfType<PlayerController>();

        // Create HUD
        Instantiate(_HUDPrefab);
    }

    private void Start()
    {
        // Init object pools
        _projectilePool = new GameObject("Projectiles").transform;
        _explosionPool = new GameObject("Explosion Particles").transform;

        _spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        // Init powerups

    }

    private Transform NewProjectile()
    {
        GameObject instance = Instantiate(_projectilePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        instance.SetActive(false);
        NetworkServer.Spawn(instance);
        instance.transform.SetParent(_projectilePool);
        return instance.transform;
    }

    private Transform NewExplosion()
    {
        GameObject instance = Instantiate(_explosionPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        NetworkServer.Spawn(instance);
        instance.transform.SetParent(_explosionPool);
        return instance.transform;
    }

    public Transform GetProjectile()
    {
        foreach (Transform projectile in _projectilePool)
        {
            if (projectile.gameObject.activeInHierarchy) continue;
            //projectile.gameObject.SetActive(true);
            return projectile;
        }
        return NewProjectile();
    }

    public Transform GetExplosion()
    {
        foreach (Transform explosion in _explosionPool)
        {
            if (explosion.gameObject.activeInHierarchy) continue;

            explosion.gameObject.SetActive(true);

            // Restart playback of the particles
            foreach (var particle in explosion.GetComponentsInChildren<ParticleSystem>())
            {
                particle.Play();
            }
            return explosion;
        }
        return NewExplosion();
    }

    public void OnPlayerGetPowerup(GameObject player, GameObject powerup)
    {
        switch (powerup.GetComponent<Powerup>().Type)
        {
            case PowerupType.Health:
                player.GetComponent<PlayerController>().AddHealth(3);
                powerup.GetComponent<Powerup>().DeactivateForSeconds(_healthCooldown);
                break;
            case PowerupType.MachineGun:
                player.GetComponent<PlayerController>().EnableMachineGun();
                powerup.GetComponent<Powerup>().DeactivateForSeconds(_minigunCooldown);
                break;
            default:
                Debug.Log("Error: unknown powerup type.");
                break;
        }
    }

    public Vector3 GetNextPowerupPosition()
    {
        Vector2 randomPos = Random.insideUnitCircle * _powerupSpawnRadius;
        return new Vector3(randomPos.x, 1f, randomPos.y);
    }

    public void FireProjectile(GameObject owner, int damage, Transform startPoint, float speed, Color color)
    {
        Transform projectile = GetProjectile();
        projectile.position = startPoint.position;
        projectile.rotation = startPoint.rotation;
        projectile.GetComponent<Projectile>().Owner = owner;
        projectile.GetComponent<Projectile>().Damage = damage;
        projectile.GetComponent<Rigidbody>().velocity = startPoint.forward * speed;
        projectile.GetComponentInChildren<ParticleSystem>().startColor = color;
        projectile.GetComponent<DisableAfterDistance>().Initialise();
        projectile.gameObject.SetActive(true);
    }
}