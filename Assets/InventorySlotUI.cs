using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
	public Image slotIcon;
	public Button displayInfoButton;
	private ItemBase target;

	public void Draw(ItemBase newTarget, InventoryUIManagement parent)
	{
		target = newTarget;

		if (target == null)
		{
			//Display empty inventory slot
			slotIcon.gameObject.SetActive(false);
		}
		else
		{
			//Display item
			slotIcon.gameObject.SetActive(true);
			slotIcon.sprite = newTarget.GetIcon();
			displayInfoButton.enabled = true;

			displayInfoButton.onClick.RemoveAllListeners();
			displayInfoButton.onClick.AddListener(() =>
			{
				parent.DisplayItemInfo(newTarget);
			});
		}
	}
}
