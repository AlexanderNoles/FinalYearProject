using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using UnityEngine;

[SimulationManagement.SimulationRoutine(-550, SimulationManagement.SimulationRoutine.RoutineTypes.Normal)]
public class PeriodChangeRoutine : RoutineBase
{
    private readonly float minimumPeriodLength = SimulationManagement.YearsToTickNumberCount(5);

    public override void Run()
    {
        //Get game world history module
        GameWorld.main.GetData(DataTags.Historical, out HistoryData historyData);

        //Get current historical period
        HistoryData.Period period = historyData.GetCurrentPeriod();

        if (period.startTick + minimumPeriodLength > SimulationManagement.currentTickID)
        {
            //Minimum period time has not passed
            return;
        }

        //Get all entites that can fight wars
        List<DataModule> warDatas = SimulationManagement.GetDataViaTag(DataTags.War);

        //Periods can be defined by a few things
        //1. Conflict
        //2. A dominant power
        //3. Some large shift in society (technological, species, etc.)

        //!  Currently there is no way to check for the 3rd case as those aspects of entities aren't modeled (03/01/2025)

        //Conflict takes priority
        //If there are large amounts of conflict (more than 50% of factions are at war) then this can be classified as a warring period

        //Then dominance of a power
        //If that power has above a threshold of territory (and has more than anyone else) then it can be classifed as dominant
        //As long as a power continues to be some amount within the the highest amount of owned territories they will still be considered dominat within this period
        //(This is done to avoid flickering between two similarly large powers, if they coexist they have similar beliefs anyway)
        //If this power undergoes some political shift than the period could also change

        float percentageAtWar = 0.0f;

        foreach (WarData warData in warDatas.Cast<WarData>())
        {
            if (warData.atWarWith.Count > 0)
            {
                percentageAtWar += 1.0f;
            }
        }

        percentageAtWar /= warDatas.Count;

        //Lower limit for conflict period, to avoid period flickering
        bool metLowerConflictThreshold = percentageAtWar >= 0.45f;
        //Actual threshold for starting conflict period
        bool metProperConflictThreshold = percentageAtWar >= 0.5f;

        if (metProperConflictThreshold) 
        {
            if (period.type != HistoryData.Period.Type.Conflict)
            {
                //Start new conflict era
                HistoryData.Period newPeriod = new HistoryData.Period();
                newPeriod.type = HistoryData.Period.Type.Conflict;
                newPeriod.color = Color.red;
                historyData.AddPeriod(newPeriod);
            }
        }
        else if (!metLowerConflictThreshold || period.type != HistoryData.Period.Type.Conflict)
        {
            //Not in conflict period or conflict period has ended
            const int periodEndThreshold = 25;
            const int periodStartThreshold = 100;
            const int relevanceThreshold = 10;

            bool inDominatedPeriod = period.type == HistoryData.Period.Type.DominantPower;

            //Get highest territory count
            int currentMax = int.MinValue;
            SimulationEntity currentMaxEntity = null;
            TerritoryData currentDominantPowerData = null;
            List<DataModule> territories = SimulationManagement.GetDataViaTag(DataTags.Territory);

            foreach (TerritoryData territory in territories.Cast<TerritoryData>())
            {
                int count = territory.territoryCenters.Count;

                if (count > currentMax)
                {
                    currentMaxEntity = territory.parent.Get();
                    currentMax = count;
                }

                if (inDominatedPeriod)
                {
                    if (period.dominantPowerID == territory.parent.Get().id)
                    {
                        //This is the current dominant power
                        currentDominantPowerData = territory;
                    }
                }
            }

            //If in dominant period and current dominant power is within relevance threshold of max
            //Then mainatin this period
            bool returnToNothingPeriod = true;
            //Check for null as dominant could have been completely removed since last tick
            if (
                currentDominantPowerData != null && 
                currentDominantPowerData.territoryCenters.Count > (currentMax - relevanceThreshold) &&
                currentDominantPowerData.territoryCenters.Count > periodEndThreshold)
            {
                returnToNothingPeriod = false;
                //If there has been some significant change in political leaning start a new period
                //Still dominated by this power

                //Get dominant power
                SimulationEntity entity = SimulationManagement.GetEntityByID(period.dominantPowerID);

                if (entity.GetData(DataTags.Political, out PoliticalData polData))
                {
                    float difference = polData.CalculatePoliticalDistance(period.originalPoliticalRep.x, period.originalPoliticalRep.y);
                    //Normalize
                    difference /= PoliticalData.maxDistance;

                    if (difference > 0.075f)
                    {
                        CreateNewDominantPowerPeriod(historyData, entity);
                    }
                }
            }
            else
            {
                //If any are above the minimum threshold then create a new period
                //With that power as the dominant one
                if (currentMaxEntity != null && currentMax > periodStartThreshold)
                {
                    returnToNothingPeriod = false;

                    CreateNewDominantPowerPeriod(historyData, currentMaxEntity);
                }
            }

            //...else go back to a none period
            if (returnToNothingPeriod && period.type != HistoryData.Period.Type.None)
            {
                HistoryData.Period newPeriod = new HistoryData.Period();
                historyData.AddPeriod(newPeriod);
            }
        }
    }

    private void CreateNewDominantPowerPeriod(HistoryData target, SimulationEntity newDominantPower)
    {
        HistoryData.Period newPeriod = new HistoryData.Period();
        newPeriod.type = HistoryData.Period.Type.DominantPower;
        newPeriod.dominantPowerID = newDominantPower.id;

        if (newDominantPower.GetData(DataTags.Emblem, out EmblemData emblemData))
        {
            newPeriod.color = emblemData.highlightColour;
        }

        if (newDominantPower.GetData(DataTags.Political, out PoliticalData polData))
        {
            newPeriod.originalPoliticalRep = new Vector2(polData.economicAxis, polData.authorityAxis);
        }

        target.AddPeriod(newPeriod);
    }
}
