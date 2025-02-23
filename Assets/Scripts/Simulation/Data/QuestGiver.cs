
using EntityAndDataDescriptor;
using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : DataModule
{
	//Stores all the active quests for this entity
	//These are only generated when the player needs them
	public int maxQuests = 3;
	private int nextQuestUpdate = -1;

	public VisitableLocation questOrigin = null;
	public List<Quest> heldQuests = new List<Quest>();

	public void OnQuestUIOpened()
	{
		if (UpdateQuests())
		{
			heldQuests.Clear();

			//Add new quests
			//Beacuse this is a game loop only side function just use generic random
			int count = Random.Range(1, maxQuests+1);

			for (int i = 0; i < count; i++)
			{
				Quest newQuest = GenerateNewQuest();

				if (newQuest != null)
				{
					heldQuests.Add(newQuest);
				}
			}

			//Every 10 minutes if we don't include warp travel's effect on time
			nextQuestUpdate = SimulationManagement.currentTickID + 200;
		}
	}

	public virtual Quest GenerateNewQuest()
	{
		//By default just make a delivery quest
		DeliveryQuest quest = new DeliveryQuest();

		//Set the quests origin
		quest.questOrigin = questOrigin;

		//Set the delivery target
		if (parent != null && parent.Get().GetData(DataTags.Settlements, out SettlementsData setData))
		{
			//Because we only want want to do one iteration through the list
			//We need to be able to skip elements
			float skipperChance = 1.0f;
			int setCount = setData.settlements.Count;

			float chanceLowerPer = 1.0f / setCount;

			foreach (SettlementsData.Settlement settlement in setData.settlements.Values)
			{
				skipperChance -= chanceLowerPer;

				if (Random.Range(0.0f, 1.0f) <= skipperChance)
				{
					//Skip this one
					continue;
				}

				if (!settlement.location.Equals(questOrigin))
				{
					//Found target
					quest.target = settlement.location;
					break;
				}
			}
		}

		if (quest.target == null)
		{
			//No other settlement found, should mean there is only one settlement
			//So can't do delivery
			return null;
		}

		return new DeliveryQuest();
	}

	public virtual bool UpdateQuests()
	{
		return nextQuestUpdate <= SimulationManagement.currentTickID;
	}
}