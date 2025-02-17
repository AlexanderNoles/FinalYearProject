using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;
using System.Linq;

[SimulationManagement.SimulationRoutine(5, SimulationManagement.SimulationRoutine.RoutineTypes.Init)]
public class EmblemInit : InitRoutineBase
{
    public override bool IsDataToInit(HashSet<Enum> tags)
    {
        return tags.Contains(DataTags.Emblem);
    }

    public override void Run()
    {
        List<DataModule> dataToInit = SimulationManagement.GetToInitData(DataTags.Emblem);

        foreach (EmblemData emblemData in dataToInit.Cast<EmblemData>())
        {
            if (emblemData.hasCreatedEmblem)
            {
                continue;
            }

            emblemData.hasCreatedEmblem = true;

			if (!emblemData.hasSetColours)
			{
				//Get the next colour from the colour rotation
				//Apply some small variation to it, so two nations colours are very rarely the same
				emblemData.mainColour = VisualDatabase.GetNextFactionColour();
				emblemData.SetColoursBasedOnMainColour();
			}

            //Get icons
            (Sprite, Sprite) icons = VisualDatabase.GetFactionIcons();
            emblemData.mainIcon = icons.Item1;
            emblemData.backingIcon = icons.Item2;
        }
    }
}
