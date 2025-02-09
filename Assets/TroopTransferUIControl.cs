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

		currentTransferAmount = Mathf.Clamp(currentTransferAmount, 0, int.MaxValue);
		inputField.SetTextWithoutNotify(currentTransferAmount.ToString());
	}

	public void Apply()
	{
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
