using UnityEngine;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour
{
    public int Damage = 1;
    public GameObject Owner;
    private Rigidbody _rigidbody;
    public Transform DesiredHeight;

    public override void OnStartServer()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject == Owner || !collider.gameObject.activeSelf)
        {
            Debug.Log((collider.gameObject == Owner) + " " + collider.gameObject.activeSelf);
            return;
        }

        var contactPoint = collider.transform.position;

        // Get explosion from pool
        Transform particle = GameManager.Instance.GetExplosion();
        particle.position = contactPoint;


        gameObject.SetActive(false);

        // If we hit a player, inflict damage
        if (collider.gameObject.CompareTag("Player"))
        {
            collider.gameObject.GetComponent<PlayerController>().TakeDamage(Damage);
        }

    }

    [ServerCallback]
    private void FixedUpdate()
    {
        // If shot by a player move projectile to a suitable height
        if (Owner.CompareTag("Player") && _rigidbody.position.y > DesiredHeight.position.y)
        {
            Vector3 desiredPosition = _rigidbody.position;
            desiredPosition.y = DesiredHeight.position.y;
            _rigidbody.position = Vector3.Lerp(_rigidbody.position, desiredPosition, 0.5f);
        }
    }
}