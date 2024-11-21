using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SimulationManagement.ActiveSimulationRoutine(0, SimulationManagement.ActiveSimulationRoutine.RoutineTypes.Init)]
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
            if (faction.GetData(Faction.Tags.Emblem, out EmblemData emblemData))
            {
                if (emblemData.hasCreatedEmblem)
                {
                    continue;
                }

                emblemData.hasCreatedEmblem = true;

                //Get the next colour from the colour rotation
                emblemData.mainColour = VisualDatabase.GetNextFactionColour();

                //Get the next icon from the icon rotation
                emblemData.icon = VisualDatabase.GetNextFactionSprite();
            }
        }
    }
}
