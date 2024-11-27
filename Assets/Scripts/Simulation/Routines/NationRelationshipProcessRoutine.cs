using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;

[SimulationManagement.ActiveSimulationRoutine(0, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Normal)]
public class NationRelationshipProcessRoutine : DebugRoutine
{
	public override void Run()
	{
		//Currently this routine will just have random change applied to nations relationships that have a chance of being huge
		//this is temporary for testing, ultimately we want a simple "working" version of the game first

		//Model relationship change
		List<Faction> nations = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Nation);

		foreach (Faction faction in nations)
		{
			if (faction.GetData(Faction.relationshipDataKey, out RelationshipData data))
			{
				foreach(KeyValuePair<int, RelationshipData.Relationship> entry in data.idToRelationship)
				{
					float change = SimulationManagement.random.Next(-101, 101) / 100.0f;
					float newValue = entry.Value.favourability + Mathf.Pow(change, 9);
					newValue = SimulationHelper.ValueTanhFalloff(newValue);
					entry.Value.favourability = newValue;

					//Model response to relationship change


					//War threshold, currently constant
					if (newValue < -0.5f)
					{
						//Each tick increase conflict value
						entry.Value.conflict = SimulationHelper.ValueTanhFalloff(entry.Value.conflict + 50, 100);

						//Currently wars are not formally declared, so we don't tell the other faction that we are in conflict
						//If we wanted to do that the below section demonstrates how to do that
						//
						//if (SimulationManagement.GetFactionByID(entry.Key).GetData(Faction.relationshipDataKey, out RelationshipData otherData))
						//{
						//	otherData.idToRelationship[faction.id].conflict = SimulationHelper.ValueTanhFalloff(otherData.idToRelationship[faction.id].conflict + 10, 100);
						//}
						//

						if (!faction.HasTag(Faction.Tags.AtWar))
						{
							//Now at war
							faction.AddTag(Faction.Tags.AtWar);
						}
					}
					else
					{
						entry.Value.conflict = Mathf.Max(entry.Value.conflict - 1, 0);
					}
				}
			}
		}
	}
}
