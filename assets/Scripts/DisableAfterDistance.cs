using UnityEngine;
using UnityEngine.Networking;

public class DisableAfterDistance : NetworkBehaviour
{
    public float Distance = 80f;
    private Vector3 _initialPosition;
    private bool _isStartingPositionSet;

    

    [ServerCallback]
    private void Update()
    {
        if (_isStartingPositionSet && Vector3.Distance(_initialPosition, transform.position) > Distance)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        _isStartingPositionSet = false;
    }
}