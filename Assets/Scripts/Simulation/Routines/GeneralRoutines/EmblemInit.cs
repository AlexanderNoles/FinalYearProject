using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(0, true)]
public class EmblemInit : InitRoutineBase
{
    public override bool TagsUpdatedCheck(HashSet<Faction.Tags> tags)
    {
        return tags.Contains(Faction.Tags.Emblem);
    }

    public override void Run()
    {
        //Get all emblem factions and gives them a random colour
        List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Emblem);

        foreach (Faction faction in factions)
        {
            if (faction.GetData(Faction.Tags.Emblem, out DataBase data))
            {
                EmblemData emblemData = data as EmblemData;
                emblemData.mainColour = 
                    new Color(
                        SimulationManagement.random.Next(0, 100) / 100.0f,
                        SimulationManagement.random.Next(0, 100) / 100.0f,
                        SimulationManagement.random.Next(0, 100) / 100.0f
                        );
            }
        }
    }
}
