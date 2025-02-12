using System.Collections.Generic;

public class PlayerInteractions : DataModule
{
    public List<Interaction> playersInteractions = new List<Interaction>();

	public List<Interaction> GetInteractionsSortedByDrawPriority()
	{
		List<Interaction> result = new List<Interaction>();

		foreach (Interaction interaction in playersInteractions)
		{
			//Find index where draw prio exceeds current element
			//Insert there
			int index;
			int ourDrawPriority = interaction.GetDrawPriority();

			for (index = 0; index < result.Count;)
			{
				if (result[index].GetDrawPriority() < ourDrawPriority)
				{
					break;
				}

				index++;
			}

			result.Insert(index, interaction);
		}

		return result;
	}
}