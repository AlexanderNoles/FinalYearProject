using EntityAndDataDescriptor;
using System.Collections.Generic;
using System.Linq;

[SimulationManagement.SimulationRoutine(-75)]
public class FeelingsMatchingRoutine : RoutineBase
{
	public override void Run()
	{
		Dictionary<int, FeelingsData> feelingsData = SimulationManagement.GetEntityIDToData<FeelingsData>(DataTags.Feelings);

		foreach (KeyValuePair<int, FeelingsData> entry in feelingsData)
		{
			FeelingsData thisFeelingsData = entry.Value;

			if (thisFeelingsData.matching)
			{
				foreach (KeyValuePair<int, FeelingsData.Relationship> relationship in thisFeelingsData.idToFeelings)
				{
					//If this other entity has feelings data
					//Match our feelings to theirs
					if (feelingsData.ContainsKey(relationship.Key))
					{
						FeelingsData oppositionFeelingsData = feelingsData[relationship.Key];

						//They have feelings about current entity...
						if (oppositionFeelingsData.idToFeelings.ContainsKey(entry.Key))
						{
							//...Then match!
							thisFeelingsData.idToFeelings[relationship.Key].favourability = oppositionFeelingsData.idToFeelings[entry.Key].favourability;
							thisFeelingsData.idToFeelings[relationship.Key].inConflict = oppositionFeelingsData.idToFeelings[entry.Key].inConflict;
						}
					}
				}
			}
		}
	}
}