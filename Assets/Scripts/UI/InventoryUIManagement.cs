using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManagement : UIState
{
	private static InventoryUIManagement instance;

	//Target of this UI
	private class InventoryTarget
	{
		public PlayerFaction targetFaction;
		public InventoryBase targetInventory;
	}

	private InventoryTarget targetData = null;
    //

    private List<SlotUI> inventorySlots = new List<SlotUI>();

    //UI Elements
    [Header("Target")]
	public GameObject mainInventoryScreen;

    protected override GameObject GetTargetObject()
    {
        return mainInventoryScreen;
    }

    public override KeyCode GetSetActiveKey()
    {
        return InputManagement.toggleInventoryKey;
    }

    protected override void OnSetActive(bool _bool)
    {
        base.OnSetActive(_bool);

		//If ui state is active do draw on main inventory page slots
		//otherwise do draw on mini inventory
		Draw(!_bool);
    }

	[Header("Mini Inventory References")]
	public RectTransform miniSlotArea;
	public MultiObjectPool miniSlotPool;

    [Header("Inventory Screen References")]
	public RectTransform mainSlotArea;
    public MultiObjectPool mainSlotPool;

    //This is the area covered by a inventory slot UI component, this includes padding
    //This can be calculated by simply getting the base slot object scale (150), dividing by 2 (75) and adding padding (+15) if need be
    private const int inventorySlotArea = 90;
	//60 / 2 = 30, 30 + 15 = 45
	private const int miniInventorySlotArea = 45;
    //

    protected override void Awake()
    {
		instance = this;
        base.Awake();
    }

    private void Update()
    {
		//Get target data
		//This is run every frame till we find the data we need
		if (targetData == null)
		{
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

					//Allow this ui state to be activated
					enabled = true;
                }
            }

			if (targetData != null)
			{
                //Do inital draw on mini inventory
                Draw(true);

                //For testing give player some random items
                for (int i = 0; i < targetData.targetInventory.GetInventoryCapacity(); i++)
                {
                    targetData.targetInventory.AddItemToInventory(new ItemBase(ItemDatabase.GetRandomItemIndex()));
                }
            }
        }
    }

	private void Draw(bool mini)
	{
		inventorySlots.Clear();

		//Create inventory slots
		if (targetData != null)
		{
			//Get inventory capacity
			int inventoryCapacity = targetData.targetInventory.GetInventoryCapacity();

			int slotScale = mini ? miniInventorySlotArea : inventorySlotArea;
			int flipLimit = mini ? 0 : 6;

            List <(float, Vector2)> outputPositions = UIHelper.CalculateSpreadPositions(inventoryCapacity, slotScale, flipLimit);

			//Actually create all inventory slots
			for (int i = 0; i < outputPositions.Count; i++)
			{
				SlotUI newSlot = null;// Instantiate(slotBaseObject, mainSlotArea).GetComponent<SlotUI>();

				//Get from target object pool
				//Retrieve component
				if (mini)
				{
					newSlot = miniSlotPool.UpdateNextObjectPosition(0, Vector3.zero).GetComponent<SlotUI>();
                }
				else
                {
                    newSlot = mainSlotPool.UpdateNextObjectPosition(0, Vector3.zero).GetComponent<SlotUI>();
                }

				//Set 3d position otherwise z component will be offset to world 0 (meaning the ui element would be inline with the render camera and not show up)
				(newSlot.transform as RectTransform).anchoredPosition3D = outputPositions[i].Item2;

				//Add slot to slot list for tracking later
				inventorySlots.Add(newSlot);

				//Then we do an initial draw on this slots
				DrawSlot(i);
			}
		}

		//Prune objects
		miniSlotPool.PruneObjectsNotUpdatedThisFrame(0);
        mainSlotPool.PruneObjectsNotUpdatedThisFrame(0);
    }

	public static void DrawSlot(int index)
	{
		if (instance == null)
		{
			return;
		}

		ItemBase target = instance.targetData.targetInventory.GetInventoryItemAtPosition(index);
		//If there is no item in that slot then this function will pass null
		//This will automatically redraw eithier the mini inventory or the big one based on what is needed
		instance.inventorySlots[index].Draw(target, () => { instance.DisplayItemInfo(target); });
	}

	public void DisplayItemInfo(ItemBase item)
	{
		if (!item.LinkedToItem())
		{
			return;
		}

		Debug.Log("Not implemented!");
	}
}
