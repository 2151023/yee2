using UnityEngine;

public class MenuController : MonoBehaviour
{
    public void SelectXbox()
    {
        MInput.Controller = MInput.ControllerType.Xbox;
    }

    public void SelectPS()
    {
        MInput.Controller = MInput.ControllerType.PS;
    }
}