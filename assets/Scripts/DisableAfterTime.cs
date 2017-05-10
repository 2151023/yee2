using UnityEngine;

public class DisableAfterTime : MonoBehaviour
{
    [SerializeField] private float _time;

    private void OnEnable()
    {
        Invoke("Disable", _time);
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }


}