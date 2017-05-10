using UnityEngine;
using UnityEngine.Networking;

public class EnemyRoaming : EnemyStatic
{
    private int _currentWaypoint = 0;
    private Transform[] _waypoints;
    private Rigidbody _rigidbody;

    [Header("Movement")]
    public float MovementSpeed = 5f;
    [Tooltip("Move to next waypoint after reaching this distance to the current target.")] public float NextWaypointDistance = 2f;

    private void Start()
    {
        // Initialise array of waypoint transforms
        var waypointParent = GameObject.Find("Waypoints").transform;

        _waypoints = new Transform[waypointParent.childCount];
        for (var i = 0; i < waypointParent.childCount; i++)
        {
            _waypoints[i] = waypointParent.GetChild(i);
        }

        _rigidbody = GetComponent<Rigidbody>();
    }

    [ServerCallback]
    private new void Update()
    {
        base.Update(); // Target closest player

        // If we didn't find a player, target nearest waypoint
        if (Target == null)
        {
            Target = GetClosestTransform(transform, _waypoints);
            _currentWaypoint = Target.GetSiblingIndex();
        }
    }

    [ServerCallback]
    private new void FixedUpdate()
    {
        base.FixedUpdate(); // Look at target and fire

        if (Target == null) return;

        // Move towards target
        _rigidbody.velocity = transform.forward * MovementSpeed;

        // Also rotate body (not rotation point)
        _rigidbody.rotation = Quaternion.Lerp(
            _rigidbody.rotation,
            Quaternion.LookRotation(Target.position + CenterOfMass - _rigidbody.position),
            Time.deltaTime * RotationSpeed);


        // For waypoints, switch targets to the next one when we get close enough
        if (Target.CompareTag("Waypoint") && Vector3.Distance(transform.position, Target.position) <
            NextWaypointDistance)
        {
            _currentWaypoint = (_currentWaypoint + 1) % _waypoints.Length;
            Target = _waypoints[_currentWaypoint];
        }
    }
}