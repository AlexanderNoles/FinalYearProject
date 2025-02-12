using EntityAndDataDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TroopTransferUIControl : PostTickUpdate
{
	private static TroopTransferUIControl instance;
	public GameObject target;
	private int controlledID;
	private SimulationEntity controlledEntityCache;
	private MilitaryData cachedMilitaryData;
	private int targetID;
	private RealSpacePosition troopTargetPos;

	private int currentTransferAmount = 0;

	public TMP_InputField inputField;

	[Header("Additional References")]
	public EmblemRenderer controlledEmblem;
	public EmblemRenderer targetEmblem;
	public DraggableWindow draggableWindow;

	private void Awake()
	{
		instance = this;
		target.SetActive(false);
	}

	public static bool IsActive()
	{
		return instance.target.activeSelf;
	}

	public static void Show(int controlledID, int targetID, RealSpacePosition targetPos)
	{
		//Reset
		instance.currentTransferAmount = 0;
		instance.inputField.SetTextWithoutNotify("0");

		//Setup
		instance.troopTargetPos = targetPos;
		instance.controlledID = controlledID;
		instance.targetID = targetID;

		instance.target.SetActive(true);

		//Setup emblem renderers
		//Get entites
		SimulationEntity controlled = SimulationManagement.GetEntityByID(controlledID);
		controlled.GetData(DataTags.Military, out instance.cachedMilitaryData);
		instance.controlledEntityCache = controlled;

		SimulationEntity target = SimulationManagement.GetEntityByID(targetID);
		//

		if (controlled.GetData(DataTags.Emblem, out EmblemData controlledED))
		{
			instance.controlledEmblem.Draw(controlledED);
		}
        else
        {
			instance.controlledEmblem.SetPureColor(Color.black);
        }

        if (target != null && target.GetData(DataTags.Emblem, out EmblemData targetED))
		{
			instance.targetEmblem.Draw(targetED);
		}
		else
		{
			instance.targetEmblem.SetPureColor(Color.black);
		}

		//Place window
		instance.draggableWindow.InitialOffset();

		//Freeze interaction cursor
		PlayerMapInteraction.FreeezeInteractionCursor(true);
		//Hide matchbook display, because it's position will no longer be updated
		PlayerMapInteraction.SetActiveMatchBookDisplay(false);
	}

	public void Close()
	{
		//Unfreeze interaction cursor
		PlayerMapInteraction.FreeezeInteractionCursor(false);
		//Let matchbook display show again
		PlayerMapInteraction.SetActiveMatchBookDisplay(true);

		//Close ui
		instance.target.SetActive(false);
	}

	public void ChangeAmount(int increment)
	{
		currentTransferAmount += increment;
		OnUpdate(false);
	}

	protected override void PostTick()
	{
		//In case reserve fleets was changed during the tick
		OnUpdate(false);
	}

	public void OnUpdate(bool autoRetrieve = true)
	{
		if (autoRetrieve)
		{
			try
			{
				currentTransferAmount = int.Parse(inputField.text);
			}
			catch (FormatException)
			{
				currentTransferAmount = 0;
			}
		}

		//Only allow transfer out of reserves
		currentTransferAmount = Mathf.Clamp(currentTransferAmount, 0, cachedMilitaryData.FullyRepairedFleetsCount(cachedMilitaryData.reserveFleets));
		inputField.SetTextWithoutNotify(currentTransferAmount.ToString());
	}

	public void Apply()
	{
		//Only get fleets with full health
		//There could be some incongriety with this and the current transfer amount if something removed a reserve for another reason (before amount was reclamped)
		//So we get the max transfer amount as a Min between those two values
		List<ShipCollection> avaliableFleets = cachedMilitaryData.GetFullyRepairedFleets(cachedMilitaryData.reserveFleets);

		int toTransferCount = Mathf.Min(currentTransferAmount, avaliableFleets.Count);

		//If sending no troops don't start a battle
		if (toTransferCount > 0)
		{
			//Get global battle data
			GameWorld.main.GetData(DataTags.GlobalBattle, out GlobalBattleData data);
			data.StartOrJoinBattle(WorldManagement.ClampPositionToGrid(troopTargetPos), troopTargetPos, controlledID, targetID, false);

			//Transfer the amount of troops to the position
			for (int i = 0; i < toTransferCount; i++)
			{
				ShipCollection fleet = avaliableFleets[i];
				if (cachedMilitaryData.RemoveFleetFromReserves(fleet))
				{
					//Fleet was in the reserves (given avaliable fleets has just been created based on reserve fleet this check should always pass)
					//Unless there is some future removal error
					cachedMilitaryData.AddFleet(troopTargetPos, fleet);
				}
			}

			//If the target is an entity and they have relationship data
			//Lower it
			if (SimulationManagement.EntityExists(targetID))
			{
				SimulationManagement.GetEntityByID(targetID).AdjustPlayerReputation(-2.0f);
			}
		}

		Close();
	}

	protected override void Update()
	{
		if (!target.activeSelf)
		{
			return;
		}

		if ((!SimulationManagement.EntityExists(targetID) && targetID != -1) || !SimulationManagement.EntityExists(controlledID))
		{
			//If eithier entity stops existing close
			Close();
		}

		base.Update();
	}
}
