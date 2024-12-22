using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManagement : MonoBehaviour
{
    private static PlayerManagement instance;
	private PlayerFaction playerFaction = null;
    private PlayerStats playerStatsTarget = null;

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

        instance.playerFaction.GetData(PlayerFaction.statDataKey, out instance.playerStatsTarget);
	}

    public static bool PlayerFactionExists()
    {
        return instance != null && GetTarget() != null;
    }

    public static PlayerFaction GetTarget()
    {
        return instance.playerFaction;
    }

    public static PlayerStats GetStats()
    {
        return instance.playerStatsTarget;
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
