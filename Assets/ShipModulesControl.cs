using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipModulesControl : MonoBehaviour
{
	private PlayerShipData targetData = null;
	private List<ModuleSlotUIControl> moduleSlotUIs = new List<ModuleSlotUIControl>();

	public InformationDisplayControl displayControl;
	public GameObject unitChoiceUI;
	private UnitChoiceUIControl unitChoiceUIControl;
	private int unitChoiceTarget;

	private void OnEnable()
	{
		unitChoiceUI.SetActive(false);

		if (targetData == null)
		{
			unitChoiceUIControl = unitChoiceUI.GetComponent<UnitChoiceUIControl>();

			//Establish target data
			List<Faction> playersFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);

			if (playersFactions.Count <= 0)
			{
				//No player faction!
				//This usually means this ui is being shown before history has finished runnning
				return;
			}

			playersFactions[0].GetData(PlayerFaction.shipDataKey, out targetData);

			moduleSlotUIs.Clear();

			foreach (Transform child in transform)
			{
				//Get first depth childern
				//If they contain a module slot
				//Add that to our management list
				if (child.TryGetComponent(out ModuleSlotUIControl newSlot))
				{
					moduleSlotUIs.Add(newSlot);
				}
			}

			unitChoiceUIControl.Setup(this);
		}


		//Iterate through all managed slots
		//Setup them up
		if (targetData == null)
		{
			return;
		}

		for (int i = 0; i < moduleSlotUIs.Count; i++)
		{
			UpdateSlot(i);
		}
	}

	private void UpdateSlot(int i)
	{
		ModuleSlotUIControl target = moduleSlotUIs[i];

		if (i < targetData.shipUnits.Count)
		{
			//Still have slots to display
			target.UpdateSlot(ModuleSlotUIControl.SlotState.Open, this, targetData.shipUnits[i], i);
		}
		else
		{
			//No slot to display
			target.UpdateSlot(ModuleSlotUIControl.SlotState.Closed, this, null, -1);
		}
	}

	public void AddUnitSlotToPlayerFactionButton()
	{
		if (targetData != null)
		{
			//Limit amount of allowed slots to current slots in UI
			if (targetData.shipUnits.Count < moduleSlotUIs.Count)
			{
				targetData.shipUnits.Add(new EmptyUnit());
				UpdateSlot(targetData.shipUnits.Count - 1);
			}
		}
	}

	public void ReplaceSelectedUnitWithUnitOfType(Type type)
	{
		if (targetData != null)
		{
			targetData.shipUnits[unitChoiceTarget] = (PlayerShipUnitBase)Activator.CreateInstance(type);

			//Redisplay the unit data
			DisplayUnitData(unitChoiceTarget);

			//Update unit slot
			UpdateSlot(unitChoiceTarget);
		}
	}

	public void DisplayUnitData(int index)
	{
		unitChoiceUI.SetActive(false);

		if (targetData != null)
		{
			if (targetData.shipUnits[index] is EmptyUnit)
			{
				//Special case of an empty unit
				//Need to display pick unit ui
				unitChoiceUI.SetActive(true);
				unitChoiceTarget = index;
			}
			else if (targetData.shipUnits[index] is IDisplay)
			{
				IDisplay unitDisplay = targetData.shipUnits[index] as IDisplay;
				displayControl.Draw(unitDisplay);
			}
		}
	}
}
