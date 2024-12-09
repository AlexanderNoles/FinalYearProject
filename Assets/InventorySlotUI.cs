using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlotUI : MonoBehaviour
{
	private ItemBase target;

	public void Draw(ItemBase newTarget)
	{
		target = newTarget;

		if (target == null)
		{
			//Display empty inventory slot
		}
		else
		{
			//Display item
		}
	}
}
