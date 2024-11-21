using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(0, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Init)]
public class EstablishInitialRelationshipsRoutine : InitRoutineBase
{
    public override bool TagsUpdatedCheck(HashSet<Faction.Tags> tags)
    {
        return true;
    }

    public override void Run()
    {
        List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Faction);
        List<(Faction, RelationshipData)> relationshipDatas = new List<(Faction, RelationshipData)>();

        foreach (Faction faction in factions)
        {
            if (faction.GetData(Faction.relationshipDataKey, out RelationshipData data))
            {
                relationshipDatas.Add((faction, data));
            }
        }

        foreach ((Faction, RelationshipData) relationshipData in relationshipDatas)
        {
            int thisFactionID = relationshipData.Item1.id;

            //First create a relationship with itself
            if (!relationshipData.Item2.idToRelationship.ContainsKey(thisFactionID))
            {
                relationshipData.Item2.idToRelationship.Add(thisFactionID, new RelationshipData.Relationship(1.0f));
            }


            if (relationshipData.Item1.HasTag(Faction.Tags.OpenRelationship))
            {
                //Tell every other faction that this one exists
                foreach ((Faction, RelationshipData) otherFaction in relationshipDatas)
                {
                    if (otherFaction.Item1.id != thisFactionID) //Not this faction
                    {
                        if (!otherFaction.Item2.idToRelationship.ContainsKey(thisFactionID)) //No established relationship
                        {
                            otherFaction.Item2.idToRelationship.Add(thisFactionID, new RelationshipData.Relationship(relationshipData.Item2.baseFavourability));
                        }
                    }
                }
            }
        }
    }
}
