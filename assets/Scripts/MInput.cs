using System;
using UnityEngine;

/// <summary>
/// Handles input from the keyboard and the selected controller.
/// Relies one the appropriate actions being defined in Unity's input manager.
/// </summary>
public static class MInput
{
    public enum ControllerType
    {
        Xbox,
        PS,
    }

    // Set default controller to Xbox
    public static ControllerType Controller = ControllerType.Xbox;

    public static float Horizontal()
    {
        return Mathf.Clamp(Input.GetAxis("Horizontal") + Input.GetAxis(Controller + "Horizontal"), -1, 1);
    }

    public static float Vertical()
    {
        return Mathf.Clamp(Input.GetAxis("Vertical") + Input.GetAxis(Controller + "Vertical"), -1, 1);
    }

    public static float TurretHorizontal()
    {
        return Input.GetAxis(Controller + "Horizontal");
    }

    public static float TurretVertical()
    {
        return Input.GetAxis(Controller + "Vertical");
    }

    public static bool Fire()
    {
        return Input.GetButton("Fire") || Input.GetButton(Controller + "Fire");
    }

    public static float Brake()
    {
        return Mathf.Clamp(Input.GetAxis("Brake") + (Input.GetAxis(Controller + "Brake")), -1, 1);
    }

// TODO: Fix hardcoded total number of weapons.
    public static int SelectWeapon(int currentWeapon)
    {
        if (Input.GetButtonDown(Controller + "WeaponNext"))
        {
            currentWeapon++;
        }
        if (Input.GetButtonDown(Controller + "WeaponPrev"))
        {
            currentWeapon--;
        }
        for (var i = 0; i < 3; i++)
        {
            if (!Input.GetButtonDown("Weapon" + i)) continue;
            currentWeapon = i;
            break;
        }
        return currentWeapon % 3;
    }

/*
public static bool NextWeapon()
{
    return Input.GetButtonDown(Controller + "WeaponNext");
}

public static bool PrevWeapon()
{
    return Input.GetButtonDown(Controller + "WeaponPrev");
}

public static int Weapon()
{
    for (var i = 0; i < 3; i++)
    {
        if (Input.GetButtonDown((string.Format("Weapon{0}", i + 1))))
        {
            return i;
        }
    }
    return -1;
}
*/
}