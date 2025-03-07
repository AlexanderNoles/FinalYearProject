using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using MonitorBreak.Bebug;
using UnityEngine;

[SimulationManagement.SimulationRoutine(27)]
public class EconomicPowerReductionRoutine : RoutineBase
{
    public override void Run()
    {
        //Get every economy data
        List<DataModule> economies = SimulationManagement.GetDataViaTag(DataTags.Economic);

        foreach (EconomyData economy in economies.Cast<EconomyData>())
        {
            float modifier = 0.0f;

            if (economy.TryGetLinkedData(DataTags.Population, out PopulationData popData))
            {
                modifier -= (popData.currentPopulationCount / 300.0f);
            }

            int randomCeilingRaise = 0;

            if (economy.TryGetLinkedData(DataTags.Strategy, out StrategyData strategyData) && strategyData is WarStrategy)
            {
                randomCeilingRaise = Mathf.FloorToInt((strategyData as WarStrategy).warExhaustion * 0.1f);
            }

            modifier *= (SimulationManagement.random.Next(-10, 51 + randomCeilingRaise) / 100.0f);

            economy.purchasingPower += modifier;

            //Final tanh clamp
            economy.purchasingPower = MathHelper.ValueTanhFalloff(economy.purchasingPower, 3000);
        }
    }
}
