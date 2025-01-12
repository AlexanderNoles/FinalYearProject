using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManagement
{
    public static KeyCode extraInformationKey = KeyCode.T;
    public static KeyCode rotateLeftKey = KeyCode.Q;
    public static KeyCode rotateRightKey = KeyCode.E;
    public static KeyCode thrusterUpKey = KeyCode.W;
    public static KeyCode thrusterDownKey = KeyCode.S;
    public static KeyCode toggleShopKey = KeyCode.G;
    public static KeyCode refuelKey = KeyCode.F;
    public static KeyCode returnToNeutral = KeyCode.Escape;
    public static KeyCode togglePauseMenu = KeyCode.Escape;
	public static KeyCode toggleMapKey = KeyCode.M;
    public static KeyCode toggleInventoryKey = KeyCode.Tab;
    public static KeyCode toggleLocationInfoKey = KeyCode.R;




    public static bool InputEnabled = true;
    public static MouseButton cameraMove = MouseButton.Right;

    public static void SetInputActive(bool _bool)
    {
        InputEnabled = _bool;
    }

    public static float playerLookSpeed = 3.0f;

    internal static Vector2 MouseInput(bool factorInLookSpeed = true, bool _override = false)
    {
        if (!InputEnabled && !_override)
        {
            return Vector2.zero;
        }

        Vector2 toReturn = new Vector2(Input.GetAxis("Mouse Y") * -1.0f, Input.GetAxis("Mouse X"));

        if (factorInLookSpeed)
        {
            toReturn *= playerLookSpeed;
        }

        return toReturn;
    }

	internal static Vector2 GetMousePosition()
	{
		return Input.mousePosition;
	}

    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }

    internal static bool GetMouseButton(MouseButton button, bool _override = false)
    {
        return Input.GetMouseButton((int)button) && (InputEnabled || _override);
    }

    internal static bool GetMouseButtonDown(MouseButton button, bool _override = false)
    {
        return Input.GetMouseButtonDown((int)button) && (InputEnabled || _override);
    }

	internal static bool GetMouseButtonUp(MouseButton button, bool _override = false)
	{
		return Input.GetMouseButtonUp((int)button) && (InputEnabled || _override);
	}

	internal static float ScrollWheelInput(bool _override = false)
    {
        if(!(_override || InputEnabled))
        {
            return 0.0f;
        }

        return Input.mouseScrollDelta.y;
    }

    internal static int GetAlphaNumberDown(int startOffset = 1, int upperExcludedClamp = 10, bool _override = false)
    {
        if (_override || InputEnabled)
        {
            int currentCode = 48; //Alpha0

            if (upperExcludedClamp > 10)
            {
                upperExcludedClamp = 10;
            }

            for (int i = startOffset; i < upperExcludedClamp; i++)
            {
                if (Input.GetKeyDown((KeyCode)currentCode+i))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    internal static Vector3 WASDInput(bool _override = false)
    {
        if (!InputEnabled && !_override)
        {
            return Vector3.zero;
        }

        return new Vector3(Input.GetAxisRaw("Vertical"),0, Input.GetAxisRaw("Horizontal"));
    }

    internal static bool GetKeyDown(KeyCode key, bool _override = false)
    {
        return Input.GetKeyDown(key) && (InputEnabled || _override);
    }

    internal static bool GetKeyUp(KeyCode key, bool _override = false)
    {
        return Input.GetKeyUp(key) && (InputEnabled || _override);
    }

    internal static bool GetKey(KeyCode key, bool _override = false)
    {
        return Input.GetKey(key) && (InputEnabled || _override);
    }
}
