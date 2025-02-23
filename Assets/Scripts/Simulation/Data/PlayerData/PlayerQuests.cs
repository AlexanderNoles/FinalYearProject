using System.Collections.Generic;

public class PlayerQuests : DataModule
{
    public List<Quest> currentQuests = new List<Quest>();
	public int maxQuestsCount = 15;

	public bool AttemptToAcceptQuest(Quest newQuest)
	{
		if (currentQuests.Count < maxQuestsCount)
		{
			currentQuests.Add(newQuest);
			return true;
		}

		return false;
	}

	public override string Read()
	{
		return $"	Quest Count: {currentQuests.Count}/{maxQuestsCount}";
	}
}