using UnityEngine;
using UnityEngine.Networking;

public enum PowerupType
{
    Health,
    MachineGun,
}

public class Powerup : NetworkBehaviour
{
    public PowerupType Type;

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(gameObject.GetInstanceID() + ": Collided with " + other.ToString());

        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.OnPlayerGetPowerup(other.gameObject, gameObject);
        }
    }

    public void DeactivateForSeconds(float time)
    {
        gameObject.SetActive(false);
        Invoke("Reactivate", time);
    }

    private void Reactivate()
    {
        gameObject.transform.position = GameManager.Instance.GetNextPowerupPosition();
        gameObject.SetActive(true);
    }
}