using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionSelectionUIManagement : MonoBehaviour
{
	private PlayerInteractions targetData = null;
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
		interactionSlots[index].Draw(target, () => { SetCurrentInteractionButtonCallback(target); });

		if (index == 0)
		{
			//Force set first interaction as current
			interactionSlots[index].ForceOnClick();
		}
	}

	public void SetCurrentInteractionButtonCallback(Interaction target)
	{
		PlayerInteractionManagement.SetCurrentInteraction(target);
	}
}
