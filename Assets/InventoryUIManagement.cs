using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.TMP_Compatibility;

public class InventoryUIManagement : MonoBehaviour
{
	//Target of this UI
	private class InventoryTarget
	{
		public PlayerFaction targetFaction;
		public InventoryBase targetInventory;
	}

	private InventoryTarget targetData = null;
	//

	//UI Elements
	public RectTransform slotArea;
	public GameObject slotBaseObject;
	public InformationDisplayControl selectedItemInformationDisplay;
	private List<SlotUI> inventorySlots = new List<SlotUI>();

	//This is the area covered by a inventory slot UI component, this includes padding
	//This can be calculated by simply getting the base slot object scale (150), dividing by 2 (75) and adding padding (+15) if need be
	private const int inventorySlotArea = 90;
	//

	private void OnEnable()
	{
		//Get data
		if (targetData == null)
		{
			//We haven't been set active yet or have been unable to find the player
			//when we have been

			//Get inventory
			List<Faction> playerOwnedFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);

			if (playerOwnedFactions.Count > 0)
			{
				//Get inventory data
				if (playerOwnedFactions[0].GetData(PlayerFaction.inventoryDataKey, out InventoryBase data))
				{
					targetData = new InventoryTarget();
					targetData.targetFaction = playerOwnedFactions[0] as PlayerFaction;
					targetData.targetInventory = data;
				}
			}
		}

		//Create inventory slots
		if (targetData != null)
		{
			//First remove any children of slot area
			foreach (Transform child in slotArea)
			{
				Destroy(child.gameObject);
			}
			//

			//Get inventory capacity
			int inventoryCapacity = targetData.targetInventory.GetInventoryCapacity();

			List<(float, Vector2)> outputPositions = UIHelper.CalculateSpreadPositions(inventoryCapacity, inventorySlotArea);

			//Actually create all inventory slots
			for (int i = 0; i < outputPositions.Count; i++)
			{
				SlotUI newSlot = Instantiate(slotBaseObject, slotArea).GetComponent<SlotUI>();
				(newSlot.transform as RectTransform).anchoredPosition = outputPositions[i].Item2;

				//Add slot to slot list for tracking later
				inventorySlots.Add(newSlot);

				//Then we do an initial draw on this slots
				//If there is no item in that slot then this function will pass null
				ItemBase target = targetData.targetInventory.GetInventoryItemAtPosition(i);
				newSlot.Draw(target, () => { DisplayItemInfo(target); });
			}
		}
	}

	public void DisplayItemInfo(ItemBase item)
	{
		if (!item.LinkedToItem())
		{
			return;
		}

		selectedItemInformationDisplay.Draw(item);
	}
}
