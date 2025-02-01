using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

public class PlayerManagement : MonoBehaviour
{
    public const bool fuelEnabled = false;
    private static PlayerManagement instance;
	private Player playerEntity = null;
    private PlayerStats playerStatsTarget = null;
    private PlayerInventory playerInventoryTarget = null;
	private PlayerInteractions playerInteractionsTarget = null;

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
		instance.playerEntity = new Player();
		instance.playerEntity.Simulate();

        instance.playerEntity.GetData(DataTags.Stats, out instance.playerStatsTarget);
        instance.playerEntity.GetData(DataTags.Inventory, out instance.playerInventoryTarget);
		instance.playerEntity.GetData(DataTags.Interactions, out instance.playerInteractionsTarget);
	}

    public static bool PlayerEntityExists()
    {
        return instance != null && GetTarget() != null;
    }

    public static Player GetTarget()
    {
        return instance.playerEntity;
    }

    public static PlayerStats GetStats()
    {
        return instance.playerStatsTarget;
    }

    public static PlayerInventory GetInventory()
    {
        return instance.playerInventoryTarget;
    }

	public static PlayerInteractions GetInteractions()
	{
		return instance.playerInteractionsTarget;
	}

	public static void KillPlayer()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    [MonitorBreak.Bebug.ConsoleCMD("shipspeed")]
    public static void IncreaseSpeed(string value)
    {
        PlayerStats stats = instance.playerStatsTarget;

        instance.playerStatsTarget.statToExtraContributors[Stats.moveSpeed.ToString()].Clear();
        instance.playerStatsTarget.statToExtraContributors[Stats.moveSpeed.ToString()].Add(new StatContributor(float.Parse(value), "debug"));
    }
}
