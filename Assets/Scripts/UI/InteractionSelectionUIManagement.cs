using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionSelectionUIManagement : MonoBehaviour
{
	private PlayerInteractions targetData = null;
	public RectTransform selectedImage;
	public List<SlotUI> interactionSlots = new List<SlotUI>();

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

		if (input != -1)
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

		int count = interactionSlots.Count;

		for (int i = 0; i < count; i++)
		{
			Interaction interaction = null;
			if (i < targetData.playersInteractions.Count)
			{
				interaction = targetData.playersInteractions[i];
			}

			DrawSlot(i, interaction);
		}
	}

	private void DrawSlot(int index, Interaction target)
	{
		interactionSlots[index].Draw(target, () => { SetCurrentInteractionButtonCallback(target, interactionSlots[index].GetComponent<RectTransform>()); });

		if (index == 0)
		{
			//Force set first interaction as current
			interactionSlots[index].ForceOnClick();
		}
	}

	public void SetCurrentInteractionButtonCallback(Interaction target, RectTransform uiRect)
	{
		PlayerInteractionManagement.SetCurrentInteraction(target, target.GetIcon());
		selectedImage.anchoredPosition3D = uiRect.anchoredPosition3D;
	}
}
