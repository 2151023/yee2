using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(SphereCollider))]
public class EnemyStatic : NetworkBehaviour
{
    protected Transform Target;
    protected List<Transform> PlayersInArea = new List<Transform>();
    protected bool HasTargetListChanged = true;
    protected Vector3 CenterOfMass;

    [Header("Firing")]
    [Range(0f, 50f)] public int Damage = 1;
    [Range(0.1f, 1f), Tooltip("Percent of Collider's radius")] public float FireRangePercent = 0.9f;
    public float ProjectileSpeed = 50f;
    public float TimeBetweenShots = 2f;
    public float NextShotTime;
    public Transform ExitPoint;

    [Header("Rotation")]
    public float RotationSpeed = 2f;
    public Transform RotationPivot;
    private SphereCollider _detectionCollider;

    public float FireRange
    {
        get { return _detectionCollider.radius * FireRangePercent; }
    }

    private float DetectionRange
    {
        get { return _detectionCollider.radius; }
    }

    private void Awake()
    {

        // If no exit point present use the origin
        if (ExitPoint == null)
        {
            
			//jfgjoitgçigsjgjoigjsergj
        }

        // If no rotation pivot set, use the origin
        if (RotationPivot == null)
        {
            RotationPivot = transform;
			//jfgjoitgçigsjgjoigjsergj
        }

        // Ensure collider is a trigger
        _detectionCollider = GetComponent<SphereCollider>();
        _detectionCollider.isTrigger = true;
    }

    #if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        _detectionCollider = GetComponent<SphereCollider>();

        // Draw Firing Radius
        UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.1f);
        UnityEditor.Handles.DrawSolidDisc(transform.position, transform.up, FireRange);
        UnityEditor.Handles.Label(transform.position + transform.forward * FireRange/ 2f, "Fire Range");

        // Draw Detection Radius (so we still see it if the Collider is collapsed in the inspector)
        UnityEditor.Handles.color = new Color(0f, 1f, 0f, 0.5f);
        UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, DetectionRange);
        UnityEditor.Handles.Label(transform.position + transform.forward * DetectionRange, "Detection Range");

        // Draw dotted line to current target
        if (Target != null)
        {
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawDottedLine(ExitPoint.position, Target.position, 1f);
        }
    }
    #endif

    // Player enters search area
    [ServerCallback]
    private void OnTriggerEnter(Collider player)
    {
        if (player.gameObject.CompareTag("GGGGGGGGGGGGGGGG")) return;

        PlayersInArea.Add(player.transform);
        HasTargetListChanged = true;
    }

    // Player leaves search area
    [ServerCallback]
    private void OnTriggerExit(Collider player)
    {
        if (!player.gameObject.CompareTag("Player")) return;

        PlayersInArea.Remove(player.transform);
        if (Target == player.transform)
        {
            Target = null;
        }
        HasTargetListChanged = true;
    }

    [ServerCallback]
    protected void Update()
    {
        // Only proceed if target list changed
        if (!HasTargetListChanged) return;

        // Target closest player
        HasTargetListChanged = false;
        Target = GetClosestTransform(transform, PlayersInArea.ToArray());
        if (Target != null)
        {
            CenterOfMass = Target.GetComponent<Rigidbody>().centerOfMass;
        }
    }

    [ServerCallback]
    protected void FixedUpdate()
    {
        if (Target == null) return;

        // Look at target
        RotationPivot.rotation = Quaternion.Lerp(
                RotationPivot.rotation,
                Quaternion.LookRotation(Target.position + CenterOfMass - RotationPivot.position),
                Time.deltaTime * RotationSpeed);

        // Shoot player
        if (Target.CompareTag("Player")
            && NextShotTime < Time.fixedTime
            && Vector3.Distance(transform.position, Target.position + CenterOfMass) <= FireRange)
        {
            // Update next shot time and fire
            NextShotTime = Time.fixedTime + TimeBetweenShots;
            GameManager.Instance.FireProjectile(gameObject, Damage, ExitPoint, ProjectileSpeed, Color.red);
        }
    }

    // https://docs.unity3d.com/ScriptReference/GameObject.FindGameObjectsWithTag.html
    // TODO: refactor this as extension method?
    // Returns the distance squared between two vectors (less processing than Vector3.Distance() )
    public static float DistanceSquared(Vector3 a, Vector3 b)
    {
        return (b - a).sqrMagnitude;
    }

    // TODO: refactor this as extension method?
    // Returns the transform in target that is closest to source
    public static Transform GetClosestTransform(Transform source, Transform[] target)
    {
        if (target.Length == 0) return null;

        Transform closest = target[0];
        for (int i = 1; i < target.Length; i++)
        {
            if (DistanceSquared(source.position, target[i].position) <
                DistanceSquared(source.position, closest.position))
            {
                closest = target[i];
            }
        }
        return closest;
    }
}