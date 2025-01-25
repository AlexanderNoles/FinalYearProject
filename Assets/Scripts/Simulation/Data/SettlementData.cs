using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettlementData : DataBase
{
    private int nextID = 0;

    public class Settlement : DataBase
    {
        public int setID;

		public class SettlementWeapon : WeaponBase
		{
			public override float GetDamageRaw()
			{
				return 1;
			}

			public override float GetTimeBetweenAttacks()
			{
				return 1.5f;
			}
		}

        public class SettlementLocation : VisitableLocation
        {
			public Shop shop;
			public SettlementWeapon weapon;
            public Settlement actualSettlement;
			private GeneratorManagement.StructureGeneration generation;

			public override void InitDraw(Transform parent)
			{
				generation = new GeneratorManagement.StructureGeneration();

				generation.parent = parent;

				generation.SpawnStructure(GeneratorManagement.POOL_INDEXES.SETTLEMENT, Vector3.zero);
				//Apply simulation context to object
				ApplyContext(generation.targets[^1].Item2);

				generation.FinalizeGeneration();
			}

			public override void Cleanup()
			{
				generation.AutoCleanup();
			}

			public override RealSpacePosition GetPosition()
            {
                return actualSettlement.actualSettlementPos;
            }

			public override float GetEntryOffset()
			{
				return 100.0f;
			}

			//UI DRAW FUNCTIONS

			public override string GetTitle()
			{
				return "Settlement";
			}

			public override string GetExtraInformation()
			{
				return $"<color={VisualDatabase.statisticColour}>Settlements</color> are important locations, from here you can buy <color={VisualDatabase.goodColourString}>Items</color> and <color={VisualDatabase.goodColourString}>Fuel</color> to support your endeavours.";
			}

			public override int GetEntityID()
			{
				return actualSettlement.parent.Get().id;
			}

			public override Shop GetShop()
			{
				return shop;
			}

			public override bool CanBuyFuel()
			{
				return true;
			}

			public override List<WeaponBase> GetWeapons()
			{
				List<WeaponBase> baseList = base.GetWeapons();
				baseList.Add(weapon);
				return baseList;
			}

			public override void OnDeath()
			{
				if(actualSettlement.parent.Get().GetData(DataTags.Settlement, out SettlementData setData))
				{
					setData.settlements.Remove(actualSettlement.setCellCenter);
				}
			}
		}

        public int maxPop = 100;
		public RealSpacePosition setCellCenter;
        public RealSpacePosition actualSettlementPos = new RealSpacePosition(0, 0, 0);
        public SettlementLocation location;

        public class TradeFleet
        {
            public int tradeFleetCapacity = 3;
            public List<TradeShip> ships = new List<TradeShip>();
        }

        public int tradeFleetCapacity = 0;
        public List<TradeFleet> tradeFleets = new List<TradeFleet>();


        public Settlement(RealSpacePosition pos, EntityLink parent)
        {
            actualSettlementPos = pos;
			//Set parent
			this.parent = parent;

            location = new SettlementLocation();
            location.actualSettlement = this;
			location.shop = new Shop();

			location.shop.capacity = 4;

			//Set shop to use same parent
			location.shop.parent = this.parent;

			//Create weapon
			location.weapon = new SettlementWeapon();
        }
    }

    public void AddSettlement(RealSpacePosition realSpacePostion, Settlement settlement)
    {
        if (settlements.ContainsKey(realSpacePostion))
        {
            return;
        }

		settlement.setCellCenter = realSpacePostion;
        settlement.setID = nextID++;
        settlements.Add(realSpacePostion, settlement);
    }

    public Dictionary<RealSpacePosition, Settlement> settlements = new Dictionary<RealSpacePosition, Settlement>();
    public int rawSettlementCapacity = 5;
}
