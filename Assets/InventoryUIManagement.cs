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
	private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();

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

					//DEBUG
					//Give player random items for testing
					for (int i = 0; i < targetData.targetInventory.GetInventoryCapacity(); i++)
					{
						targetData.targetInventory.AddItemToInventory(new ItemBase(ItemDatabase.GetRandomItemIndex()));
					}

					//Run absent routine
					SimulationManagement.RunAbsentRoutine("PlayerStatsCalculation");

					//Output stats data
					playerOwnedFactions[0].GetData(PlayerFaction.statDataKey, out PlayerStats statData);
					Debug.Log(statData.GetStat(Stats.health.ToString()));
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

			//!Here we should really be finding the pair of factors that have a minimum distance from each other
			//So we don't end up with shapes that are stretched looking in one axis

			//Establish the two highest factors
			List<int> factors = new List<int>();
			int visualInventoryCapacity = inventoryCapacity - (inventoryCapacity % 2);

			int max = Mathf.FloorToInt(Mathf.Sqrt(visualInventoryCapacity));

			for (int potentialFactor = max; potentialFactor >= 1 && factors.Count < 2; potentialFactor--)
			{
				if (visualInventoryCapacity % potentialFactor == 0) //Is a factor
				{
					factors.Add(potentialFactor);

					//Don't add the square root twice
					if (potentialFactor != visualInventoryCapacity / potentialFactor)
					{
						factors.Add(visualInventoryCapacity / potentialFactor);
					}
				}
			}

			if (factors.Count == 0)
			{
				//This indicates the inventory is of space <= 0
				return;
			}

			List<(float, Vector2)> outputPositions = new List<(float, Vector2)>();

			//Find max factor for y and min factor for x, this is done instead of simply taking the first and last element of the list
			//because that doesn't work for odd sized inventories

			int yLimit = 0, xLimit = -1;

			foreach (int factor in factors)
			{
				int currentHighest = Mathf.Max(yLimit, factor);

				if (currentHighest != yLimit)
				{
					xLimit = yLimit;
					yLimit = currentHighest;
				}
			}

			//Need to perform two (one positive, one negative) iterations per each rounded half of the limits
			//we stop immeditely if the number of iterations reaches the limit
			int totalYIterations = 0;
			for (int y = Mathf.FloorToInt(yLimit / 2.0f); y >= 0 && totalYIterations < yLimit; y--)
			{
				//One negative, one positive
				for (int i = -1; i <= 1 && totalYIterations < yLimit; i += 2)
				{
					int totalXIterations = 0;
					for (int x = Mathf.FloorToInt(xLimit / 2.0f); x >= 0 && totalXIterations < xLimit; x--)
					{
						//One negative, one positive
						for (int j = -1; j <= 1 && totalXIterations < xLimit; j += 2)
						{
							//Add slot at calculated position
							//Add some extra displacement if the slot count is an even number (as the there will be no true center slot)
							Vector2 anchoredPosition = new Vector2(
								(x * j * (inventorySlotArea * 2)) - (Mathf.Clamp01(x) * j * (1.0f - (xLimit % 2)) * inventorySlotArea),
								(y * i * (inventorySlotArea * 2)) - (Mathf.Clamp01(y) * i * (1.0f - (yLimit % 2)) * inventorySlotArea)
								);

							//Add to output positions
							//Need to order the positions so the inventory slots have a defined sensible order
							//This is fairly simple, we simply need to perform a simplistic priority add operation that use the position
							//priority is calculated as so: (y * 1000.0f) + (-1.0f * x).
							//This enforces a "top to bottom, left to right order", as the y is massively weighted and the x is inverted

							float priority = (anchoredPosition.y * 1000.0f) + (-1.0f * anchoredPosition.x);

							//Default first position
							int addIndex = 0;

							for (addIndex = 0; addIndex < outputPositions.Count; addIndex++)
							{
								if (priority > outputPositions[addIndex].Item1)
								{
									//Higher priority than this position
									//We have found our insert placement
									break;
								}
							}

							outputPositions.Insert(addIndex, (priority, anchoredPosition));

							//Increment total x iterations
							totalXIterations++;
						}
					}

					//Increment total y iterations
					totalYIterations++;
				}
			}

			//Actually create all inventory slots
			for (int i = 0; i < outputPositions.Count; i++)
			{
				InventorySlotUI newSlot = Instantiate(slotBaseObject, slotArea).GetComponent<InventorySlotUI>();
				(newSlot.transform as RectTransform).anchoredPosition = outputPositions[i].Item2;

				//Add slot to slot list for tracking later
				inventorySlots.Add(newSlot);

				//Then we do an initial draw on this slots
				//If there is no item in that slot then this function will pass null
				newSlot.Draw(targetData.targetInventory.GetInventoryItemAtPosition(i));
			}
		}
	}
}
