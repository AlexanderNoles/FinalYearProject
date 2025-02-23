using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;

public class PlayerManagement : PostTickUpdate
{
    public const bool fuelEnabled = false;
    private static PlayerManagement instance;
	private Player playerEntity = null;
    private PlayerStats playerStatsTarget = null;
    private PlayerInventory playerInventoryTarget = null;
	private PlayerInteractions playerInteractionsTarget = null;
	private PlayerQuests playerQuestsTarget = null;
	private PopulationData playerPopulationTarget = null;
	private MilitaryData playerMilitaryTarget = null;

	private void Awake()
    {
        instance = this;

        Cursor.lockState = CursorLockMode.Confined;
    }

	protected override void PostTick()
	{
		if (PlayerEntityExists())
		{
			//Process current quests
			for (int i = 0; i < playerQuestsTarget.currentQuests.Count;)
			{
				if (!playerQuestsTarget.currentQuests[i].PostTickValidate())
				{
					//Validation failed, quest is no longer completable
					playerQuestsTarget.currentQuests.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}
	}

	protected override void Update()
	{
		base.Update();

		if (PlayerEntityExists())
		{
			//Process current quests
			for (int i = 0; i < playerQuestsTarget.currentQuests.Count;)
			{
				if (playerQuestsTarget.currentQuests[i].CompletedCheck())
				{
					//Quest completed
					playerQuestsTarget.currentQuests[i].ApplyReward();
					playerQuestsTarget.currentQuests.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}
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
		instance.playerEntity.GetData(DataTags.Population, out instance.playerPopulationTarget);
		instance.playerEntity.GetData(DataTags.Military, out instance.playerMilitaryTarget);
		instance.playerEntity.GetData(DataTags.Quests, out instance.playerQuestsTarget);
	}

    public static bool PlayerEntityExists()
    {
        return instance != null && GetTarget() != null;
    }

    public static Player GetTarget()
    {
		if (instance == null || instance.playerEntity == null)
		{
			return null;
		}

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

	public static PopulationData GetPopulation()
	{
		return instance.playerPopulationTarget;
	}

	public static MilitaryData GetMilitary()
	{
		return instance.playerMilitaryTarget;
	}

	public static PlayerQuests GetQuests()
	{
		return instance.playerQuestsTarget;
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
