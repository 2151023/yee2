
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private GameObject _weaponBar;
    [SerializeField] private Text _speedLabel;
    [SerializeField] private Text _mgTimer;
    [SerializeField] private GameObject _scoreboard;

    public void UpdateSelectedWeapon(int selectedWeapon)
    {
        for (var i = 0; i < _weaponBar.transform.childCount; i++)
        {
            var color = _weaponBar.transform.GetChild(i).GetComponent<Image>().color;
            color.a = (selectedWeapon == i ? 1f : 0f);
            _weaponBar.transform.GetChild(i).GetComponent<Image>().color = color;
        }
    }

    public void UpdateMachineGunTimer(float time)
    {
        _mgTimer.text = time.ToString("0.0s");
    }

    public void UpdateSpeed(float speed)
    {
        _speedLabel.text = speed.ToString("0.0");
    }


}