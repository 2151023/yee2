using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HealthBarController : MonoBehaviour
{

    [SerializeField] private GameObject _hitpointPrefab;

    public void SetHealth(int newHealth, int maxHealth)
    {
        // Define color for images
        Color newColor = Color.Lerp(Color.red, Color.green, (float)newHealth / maxHealth );

        // If max health mismatches hitpoint graphics, fix it
        if (transform.childCount != maxHealth)
        {
            // Destroy current children
            for (int i = 0; i < transform.childCount; i++) {
                Destroy(transform.GetChild(i).gameObject);
            }

            // Recreate them
            for (int i = 0; i < maxHealth; i++)
            {
                GameObject currentHitpoint = Instantiate(_hitpointPrefab, transform) as GameObject;
                currentHitpoint.transform.localRotation = Quaternion.identity;
                currentHitpoint.transform.localPosition = Vector3.zero;
                currentHitpoint.transform.SetParent(transform, false);
            }
        }

        // Iterate images and enable/set color as needed
        for (int i = 0; i < maxHealth; i++)
        {
            Transform hitpoint = transform.GetChild(i);
            hitpoint.GetComponent<Image>().color = newColor;
            hitpoint.gameObject.SetActive(i < newHealth);
        }
    }
}
