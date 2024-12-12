using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
	public Image slotIcon;
	public Button button;
	private ItemBase target;

	public void Draw(ItemBase newTarget, UnityAction onButtonClick)
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
			button.enabled = true;

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(onButtonClick);
		}
	}
}
