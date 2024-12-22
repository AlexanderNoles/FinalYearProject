using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManagement : MonoBehaviour
{
    private static PlayerManagement instance;
	private PlayerFaction playerFaction;

    private void Awake()
    {
        instance = this;

        Cursor.lockState = CursorLockMode.Confined;
    }

    public static Vector3 GetPosition()
    {
        return instance.transform.position;
    }

	public static void InitPlayerFaction()
	{
		instance.playerFaction = new PlayerFaction();
		instance.playerFaction.Simulate();
	}

    public static void KillPlayer()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
