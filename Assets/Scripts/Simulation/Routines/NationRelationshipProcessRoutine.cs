using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;

[SimulationManagement.ActiveSimulationRoutine(10, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Normal)]
public class NationRelationshipProcessRoutine : RoutineBase
{
	public override void Run()
	{
		//Get all factions and their relationship data
		List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Faction);
		List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Nation);
		Dictionary<int, RelationshipData> idToRelationship = SimulationManagement.GetDataForFactionsList<RelationshipData>(factions, Faction.relationshipDataKey);
		//Get political data for factions that have it (this is important for how nations view other nations)
		Dictionary<int, PoliticalData> idToPolitics = SimulationManagement.GetDataForFactionsList<PoliticalData>(factions, Faction.Tags.Politics.ToString());
		Dictionary<int, WarData> idToWar = SimulationManagement.GetDataForFactionsList<WarData>(factions, Faction.Tags.CanFightWars.ToString());

		int count = nations.Count;
		for (int i = 0; i < count; i++)
		{
			Nation nation = nations[i] as Nation;

			RelationshipData relationshipData = idToRelationship[nation.id];
			PoliticalData personalPoliticalData = idToPolitics[nation.id];
			WarData personalWarData = idToWar[nation.id];

			foreach (KeyValuePair<int, RelationshipData.Relationship> entry in relationshipData.idToRelationship)
			{
				RelationshipData.Relationship relationship = entry.Value;

				float newInstability = RelationshipData.Relationship.baseInstability;
				float difference = 0.0f;

				if (idToPolitics.ContainsKey(entry.Key))
				{
					difference = idToPolitics[entry.Key].CalculatePoliticalDistance(personalPoliticalData.economicAxis, personalPoliticalData.authorityAxis);

					//Normalize
					difference /= PoliticalData.maxDistance;

					//Shit into negative to positive (-1 to 1) range
					difference -= 0.5f;
					difference *= 2;

					newInstability *= Mathf.Abs(difference);
				}

				//Reduce instability if favourability is above a certain value and difference is negative
				if (relationship.favourability > 0.4f && difference < 0.0f)
				{
					newInstability /= 2;
				}

				const float changePerTickModifier = 0.2f;
				relationship.favourability = Mathf.MoveTowards(relationship.favourability, difference, newInstability * changePerTickModifier);

				if (relationship.favourability < -0.5f)
				{
					if (!personalWarData.atWarWith.Contains(entry.Key))
					{
						relationship.inConflict = true;

						if (!SimulationManagement.GetFactionByID(entry.Key).HasTag(Faction.Tags.Insignificant))
						{
							//If this faction is not insignificant
							//Start a dedicated war
							personalWarData.atWarWith.Add(entry.Key);
						}
					}
				}
			}
		}
	}
}
