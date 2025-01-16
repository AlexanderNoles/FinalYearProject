using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;
using System;
using System.Linq;

[SimulationManagement.SimulationRoutine(0, SimulationManagement.SimulationRoutine.RoutineTypes.Init)]
public class FeelingsInit : InitRoutineBase
{
    public override bool IsDataToInit(HashSet<Enum> tags)
    {
        return tags.Contains(DataTags.Feelings);
    }

    public override void Run()
    {
        //Get all feelings data
        //(Not just the ones being initalized)
        List<FeelingsData> allFeelingsData = SimulationManagement.GetDataViaTag(DataTags.Feelings).Cast<FeelingsData>().ToList();
        //Then get all feelings data that needs to be initlaized
        List<DataBase> toInitFeelingsData = SimulationManagement.GetToInitData(DataTags.Feelings);

        foreach (FeelingsData feelingsData in toInitFeelingsData.Cast<FeelingsData>())
        {
            int thisEntityID = feelingsData.parent.Get().id;

            if (feelingsData.TryGetLinkedData(DataTags.ContactPolicy, out ContactPolicyData contactData))
            {
                //Is this entity immediately visible to all other entities?
                if (!contactData.visibleToAll)
                {
                    continue;
                }

                //Tell every other entity this one exists
				//And add feelings about that entity to this one
                foreach (FeelingsData otherEntityFeelings in allFeelingsData)
                {
					int otherID = otherEntityFeelings.parent.Get().id;
					//Can't have a relationship with itself!
					if (otherID != thisEntityID)
                    {
                        if (!otherEntityFeelings.idToFeelings.ContainsKey(thisEntityID)) //No established relationship
                        {
                            otherEntityFeelings.idToFeelings.Add(thisEntityID, new FeelingsData.Relationship(feelingsData.baseFavourability));
                        }

						if (!feelingsData.idToFeelings.ContainsKey(otherID))
						{
							feelingsData.idToFeelings.Add(otherID, new FeelingsData.Relationship(otherEntityFeelings.baseFavourability));
						}
                    }
                }
            }
        }
    }
}
