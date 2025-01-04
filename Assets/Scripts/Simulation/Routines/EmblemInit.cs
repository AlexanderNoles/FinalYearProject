using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;
using System.Linq;

[SimulationManagement.SimulationRoutine(0, SimulationManagement.SimulationRoutine.RoutineTypes.Init)]
public class EmblemInit : InitRoutineBase
{
    public override bool IsDataToInit(HashSet<Enum> tags)
    {
        return tags.Contains(DataTags.Emblem);
    }

    public override void Run()
    {
        List<DataBase> dataToInit = SimulationManagement.GetToInitData(DataTags.Emblem);

        foreach (EmblemData emblemData in dataToInit.Cast<EmblemData>())
        {
            if (emblemData.hasCreatedEmblem)
            {
                continue;
            }

            emblemData.hasCreatedEmblem = true;

            //Get the next colour from the colour rotation
            emblemData.mainColour = VisualDatabase.GetNextFactionColour();
            emblemData.highlightColour = emblemData.mainColour * 2.0f;
            emblemData.shadowColour = emblemData.mainColour * 0.25f;

            //Get icons
            (Sprite, Sprite) icons = VisualDatabase.GetFactionIcons();
            emblemData.mainIcon = icons.Item1;
            emblemData.backingIcon = icons.Item2;
        }
    }
}
