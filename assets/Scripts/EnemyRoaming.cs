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

    
}