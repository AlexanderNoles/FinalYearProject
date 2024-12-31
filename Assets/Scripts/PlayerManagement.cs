using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

public class PlayerManagement : MonoBehaviour
{
    public const bool fuelEnabled = false;
    private static PlayerManagement instance;
	private Player playerFaction = null;
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
		instance.playerFaction = new Player();
		instance.playerFaction.Simulate();

        instance.playerFaction.GetData(DataTags.Stats, out instance.playerStatsTarget);
	}

    public static bool PlayerFactionExists()
    {
        return instance != null && GetTarget() != null;
    }

    public static Player GetTarget()
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
