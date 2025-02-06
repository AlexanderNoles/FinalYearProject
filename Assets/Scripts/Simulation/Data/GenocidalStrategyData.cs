using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenocidalStrategyData : StrategyData
{
	public override List<int> GetTargets()
	{
		//Get every entity that has territory
		//a.k.a anyone we can attack

		List<DataModule> terrData = SimulationManagement.GetDataViaTag(DataTags.Territory);

		List<int> toReturn = new List<int>();

		foreach (DataModule territory in terrData)
		{
			int id = territory.parent.Get().id;

			if (id == parent.Get().id)
			{
				//Can't target ourself
				continue;
			}

			toReturn.Add(id);
		}

		return toReturn;
	}
}
