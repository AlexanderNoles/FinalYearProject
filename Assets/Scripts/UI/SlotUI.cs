using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
	public Image slotIcon;
	public Button button;
	public bool buttonEnabled = true;
	public DisplayOnHover onHover;

	public void Draw(IDisplay newTarget, UnityAction onButtonClick)
	{
		if (newTarget == null)
		{
			//Display empty slot
			slotIcon.gameObject.SetActive(false);
			button.enabled = false;
		}
		else
		{
			//Display item
			slotIcon.gameObject.SetActive(true);
			slotIcon.sprite = newTarget.GetIcon();
			button.enabled = buttonEnabled;

			if (onHover != null)
			{
				onHover.Setup(newTarget);
			}

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(onButtonClick);
		}
	}

	public void ForceOnClick()
	{
		if (button.enabled)
		{
			button.onClick.Invoke();
		}
	}
}
