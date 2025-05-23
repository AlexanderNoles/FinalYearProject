using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak.Bebug;
using EntityAndDataDescriptor;
using System.Linq;

[SimulationManagement.SimulationRoutine(10)]
public class PoliticalEffectOnFeelingsRoutine : RoutineBase
{
	public override void Run()
	{
		//Get all feelings data
		//If those entites have political data then alter their feelings about other entites
		//that have political data

		List<DataModule> feelingsDatas = SimulationManagement.GetDataViaTag(DataTags.Feelings);
		Dictionary<int, PoliticalData> idToPolitics = SimulationManagement.GetEntityIDToData<PoliticalData>(DataTags.Political);

		foreach (FeelingsData feelingsData in feelingsDatas.Cast<FeelingsData>())
		{
            WarStrategy personalWarData = null;
            bool canStartWars = feelingsData.TryGetLinkedData(DataTags.Strategy, out StrategyData strategyData) && strategyData is WarStrategy;

			if (canStartWars)
			{
				personalWarData = (WarStrategy)strategyData;
			}

			if (feelingsData.TryGetLinkedData(DataTags.Political, out PoliticalData personalPoliticalData))
			{
				foreach (KeyValuePair<int, FeelingsData.Relationship> entry in feelingsData.idToFeelings)
				{
                    FeelingsData.Relationship relationship = entry.Value;

                    float newInstability = FeelingsData.Relationship.baseInstability;
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
					else
					{
						newInstability = 0.0f;
					}

                    //Reduce instability if favourability is above a certain value and difference is negative
                    if (relationship.favourability > 0.4f && difference < 0.0f)
                    {
                        newInstability /= 2;
                    }

                    const float changePerTickModifier = 0.2f;
                    relationship.favourability = Mathf.MoveTowards(relationship.favourability, difference, newInstability * changePerTickModifier);

                    if (relationship.favourability < BalanceManagement.oppositionThreshold && canStartWars)
                    {
                        if (!personalWarData.atWarWith.Contains(entry.Key))
                        {
                            relationship.inConflict = true;

                            if (!SimulationManagement.GetEntityByID(entry.Key).HasTag(EntityStateTags.Insignificant))
                            {
                                //If this entity is not insignificant
                                //Start a dedicated war
                                personalWarData.atWarWith.Add(entry.Key);
                            }
                        }
                    }
                }
			}
		}
	}
}
