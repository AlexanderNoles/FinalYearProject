using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettlementData : DataBase
{
    private int nextID = 0;

    public class Settlement
    {
        public int setID;

        public class SettlementLocation : VisitableLocation
        {
            public Settlement actualSettlement;
			private GeneratorManagement.Generation generation;

			public override void InitDraw()
			{
				//Core generator
				generation = new GeneratorManagement.StructureGeneration().SpawnStructure(GeneratorManagement.STRUCTURES_INDEXES.SETTLEMENT, Vector3.zero);
				generation.FinalizeGeneration();
			}

			public override void Cleanup()
			{
				generation.AutoCleanup();
			}

			public override RealSpacePostion GetPosition()
            {
                return actualSettlement.actualSettlementPos;
            }

			public override float GetEntryOffset()
			{
				return 100.0f;
			}
		}


        public int maxPop = 100;
        public RealSpacePostion actualSettlementPos = new RealSpacePostion(0, 0, 0);
        public SettlementLocation location;

        public class TradeFleet
        {
            public int tradeFleetCapacity = 3;
            public List<TradeShip> ships = new List<TradeShip>();
        }

        public int tradeFleetCapacity = 0;
        public List<TradeFleet> tradeFleets = new List<TradeFleet>();


        public Settlement(RealSpacePostion pos)
        {
            actualSettlementPos = pos;


            location = new SettlementLocation();
            location.actualSettlement = this;
        }
    }

    public void AddSettlement(RealSpacePostion realSpacePostion, Settlement settlement)
    {
        if (settlements.ContainsKey(realSpacePostion))
        {
            return;
        }

        settlement.setID = nextID++;
        settlements.Add(realSpacePostion, settlement);
    }

    public Dictionary<RealSpacePostion, Settlement> settlements = new Dictionary<RealSpacePostion, Settlement>();
    public int rawSettlementCapacity = 5;
}
