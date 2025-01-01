using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManagement : UIState
{
	private static InventoryUIManagement instance;

	//Target of this UI
	private class InventoryTarget
	{
		public Player targetFaction;
		public InventoryBase targetInventory;
	}

	private InventoryTarget targetData = null;
    //

    private List<SlotUI> inventorySlots = new List<SlotUI>();
	private bool centerDictDrawn = false;

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

		if (_bool && !centerDictDrawn)
		{
			DrawDictionary();
		}

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
    public MultiObjectPool dictionarySlotPool;


    //This is the area covered by a inventory slot UI component, this includes padding
    //This can be calculated by simply getting the base slot object scale (150), dividing by 2 (75) and adding padding (+15) if need be
    private const int inventorySlotArea = 90;
	//60 / 2 = 30, 30 + 15 = 45
	private const int miniInventorySlotArea = 45;
	//
	private const float dictionarySlotSize = 75;
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
			if (PlayerManagement.PlayerEntityExists())
			{
                targetData = new InventoryTarget();
                targetData.targetFaction = PlayerManagement.GetTarget();
                targetData.targetInventory = PlayerManagement.GetInventory();

                //Allow this ui state to be activated
                enabled = true;
            }

			if (targetData != null)
			{
                //Do inital draw on mini inventory
                Draw(true);
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

            //Prune objects
            miniSlotPool.PruneObjectsNotUpdatedThisFrame(0);
            mainSlotPool.PruneObjectsNotUpdatedThisFrame(0);
        }
    }

	public void DrawDictionary()
	{
		centerDictDrawn = true;

		string itemTypeString = "";
		int allItemsCount = ItemDatabase.GetItemCount();
		//Slot area is always a constant amount of units wide
		float fullDivision = 550.0f / dictionarySlotSize;
        int entriesPerRow = Mathf.FloorToInt(fullDivision);
		float buffer = ((fullDivision - entriesPerRow) * dictionarySlotSize);

		int rows = 0;
		int indexInRow = 0;

        for (int i = 0; i < allItemsCount; i++)
		{
			ItemDatabase.ItemData item = ItemDatabase.GetItem(i);
            if (!item.itemTypeDeclaration.Equals(itemTypeString))
			{
				itemTypeString = item.itemTypeDeclaration;
                //Add an additional additional row if past the first
                if (rows > 0)
                {
                    rows++;
                }

				//Add label in this position
				Transform label = dictionarySlotPool.UpdateNextObjectPosition(1, Vector3.zero);
				(label as RectTransform).anchoredPosition3D = new Vector2(buffer, -(rows * dictionarySlotSize));

                label.GetChild(0).GetComponent<TextMeshProUGUI>().text = itemTypeString;

                //Offset down by one
                rows++;
				//Reset indexInRow
				indexInRow = 0;
			}

			if (indexInRow >= entriesPerRow)
			{
				indexInRow = 0;
				rows++;
			}

			//As offset from the top left corner
			Vector2 position = new Vector2();
			position.x = (indexInRow * dictionarySlotSize) + buffer;
			position.y = -(rows * dictionarySlotSize);

			Transform target = dictionarySlotPool.UpdateNextObjectPosition(0, Vector3.zero);
			(target as RectTransform).anchoredPosition3D = position;

			//Get target image component
			target.GetChild(0).GetComponent<Image>().sprite = item.icon;

			//Increment index in row
			indexInRow++;
		}
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
