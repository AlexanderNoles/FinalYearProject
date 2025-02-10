using EntityAndDataDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TroopTransferUIControl : MonoBehaviour
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
		currentTransferAmount = Mathf.Clamp(currentTransferAmount, 0, cachedMilitaryData.reserveFleets.Count);
		inputField.SetTextWithoutNotify(currentTransferAmount.ToString());
	}

	public void Apply()
	{
		//Get global battle data
		GameWorld.main.GetData(DataTags.GlobalBattle, out GlobalBattleData data);
		data.StartOrJoinBattle(WorldManagement.ClampPositionToGrid(troopTargetPos), troopTargetPos, controlledID, targetID, false);

		//Transfer the amount of troops to the position
		for (int i = 0; i < currentTransferAmount; i++)
		{
			ShipCollection fleet = cachedMilitaryData.RemoveFleetFromReserves();

			if (fleet != null)
			{
				cachedMilitaryData.AddFleet(troopTargetPos, fleet);
			}
		}

		//If the target is an entity and they have relationship data
		//Lower it
		if (SimulationManagement.EntityExists(targetID))
		{
			SimulationManagement.GetEntityByID(targetID).AdjustPlayerReputation(-2.0f);
		}

		//Should create a recorded battle here so we can determine when the player is allowed to retreat
		//And also to auto add ships back to reserves after battles are over

		Close();
	}

	private void Update()
	{
		if (target.activeSelf && ((!SimulationManagement.EntityExists(targetID) && targetID != -1) || !SimulationManagement.EntityExists(controlledID)))
		{
			//If eithier entity stops existing close
			Close();
		}
	}
}
