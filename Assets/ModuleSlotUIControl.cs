using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModuleSlotUIControl : MonoBehaviour
{
	public enum SlotState
	{
		Open,
		Closed
	}

	private SlotState state;
	private int slotIndex;

	public Button mainSlotButton;
	public GameObject slotClosedUI;

	public Image slotIconImage;

	public void UpdateSlot(SlotState currentState, ShipModulesControl parent, PlayerShipUnitBase unitInSlot, int newSlotIndex)
	{
		slotIndex = newSlotIndex;
		state = currentState;

		if (state == SlotState.Open)
		{
			//Showcase slot based on what unit is here
			slotClosedUI.SetActive(false);

			Sprite newSprite = VisualDatabase.GetUnitSpriteFromType(unitInSlot.GetType());

			if (newSprite != null)
			{
				slotIconImage.gameObject.SetActive(true);
				slotIconImage.sprite = newSprite;
			}
			else
			{
				slotIconImage.gameObject.SetActive(false);
			}

			//Setup button
			mainSlotButton.onClick.RemoveAllListeners();
			mainSlotButton.onClick.AddListener(delegate
			{
				parent.DisplayUnitData(slotIndex);
			});
		}
		else if (state == SlotState.Closed)
		{
			//Showcase locked slot
			slotClosedUI.SetActive(true);
			slotIconImage.gameObject.SetActive(false);
		}
	}
}
