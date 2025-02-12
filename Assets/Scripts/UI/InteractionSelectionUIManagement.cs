using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionSelectionUIManagement : MonoBehaviour
{
	private PlayerInteractions targetData = null;
	public RectTransform selectedImage;
	private GameObject selectedImageGO;
	public List<SlotUI> interactionSlots = new List<SlotUI>();

	private void Awake()
	{
		selectedImageGO = selectedImage.gameObject;
	}

	private void Update()
	{
		if (targetData == null)
		{
			if (PlayerManagement.PlayerEntityExists())
			{
				targetData = PlayerManagement.GetInteractions();

				//Do inital draw
				Draw();
			}
		}

		//Number key inputs
		int input = InputManagement.GetAlphaNumberDown();

		//Because the troop transfer has an input field we don't want to change interaction when a number is input to it
		//Ideally we would have the input field consume the input but can't figure out how to do that
		//Mainly because there is no way to insure the input field will go first in the frame and take prio over the interaction change
		if (input != -1 && !TroopTransferUIControl.IsActive())
		{
			int index = input - 1;

			if (index < interactionSlots.Count)
			{
				interactionSlots[index].ForceOnClick();
			}
		}
	}

	private void Draw()
	{
		if (targetData == null)
		{
			return;
		}

		List<Interaction> sortedInteractions = targetData.GetInteractionsSortedByDrawPriority();
		int count = interactionSlots.Count;

		for (int i = 0; i < count; i++)
		{
			Interaction interaction = null;
			if (i < sortedInteractions.Count)
			{
				interaction = sortedInteractions[i];
			}

			DrawSlot(i, interaction);
		}

		PlayerInteractionManagement.EnableSmartInteraction(true);
		selectedImageGO.SetActive(false);
	}

	private void DrawSlot(int index, Interaction target)
	{
		interactionSlots[index].Draw(target, () => { SetCurrentInteractionButtonCallback(target, interactionSlots[index].GetComponent<RectTransform>()); });
	}

	public void SetCurrentInteractionButtonCallback(Interaction target, RectTransform uiRect)
	{
		if (target.Equals(PlayerInteractionManagement.GetCurrentInteraction()))
		{
			PlayerInteractionManagement.EnableSmartInteraction(true);

			//Hide selected graphic
			selectedImageGO.SetActive(false);
		}
		else
		{
			PlayerInteractionManagement.SetCurrentInteraction(target);
			selectedImageGO.SetActive(true);
			selectedImage.anchoredPosition3D = uiRect.anchoredPosition3D;
		}
	}
}
