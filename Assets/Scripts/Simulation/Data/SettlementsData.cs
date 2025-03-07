using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Copied from discord (27/02/2025):
//Gameplay outline for attacking settlements:
//Nation Settlements are meant to have a soft time limit
//Eventually too many other ships will arrive and kill you
//Your objective is to destroy the settlement and get out before you die
//Several factors could impact this
//Things within the settlement. For example, larger cannons that will kill you if you don't deal with them first, a large shield generator, etc.
//Things outside the settlement. If the Nation is at war, or otherwise involved in a huge amount of battles then they can't commit as many ships to defence
//Your own ships being able to cover for you and soak / deal damage before the forces become overwhelming
//because of the tuning of nations they will typically always eventually overpower you but there is some wiggle room there

public class SettlementsData : DataModule
{
    private int nextID = 0;

    public class Settlement : DataModule
    {
        public int setID;
		public int maxPop = 100;
		public RealSpacePosition setCellCenter;
		public RealSpacePosition actualSettlementPos = new RealSpacePosition(0, 0, 0);
		public SettlementLocation location;
		public RefineryData settlementRefinery;

		public class TradeFleet
		{
			public int tradeFleetCapacity = 3;
			public List<TradeShip> ships = new List<TradeShip>();
		}

		public int tradeFleetCapacity = 0;
		public List<TradeFleet> tradeFleets = new List<TradeFleet>();

		public class SettlementWeapon : StandardSimWeaponProfile
		{
			public override float ShotsPerAttack()
			{
				return 10;
			}

			public override float GetDamageRaw()
			{
				return 1;
			}

			public override float GetTimeBetweenAttacks()
			{
				return 0.25f;
			}
		}

        public class SettlementLocation : VisitableLocation
        {
			public Shop shop;
			public QuestGiverData questGiver;
			public SettlementWeapon weapon;
            public Settlement actualSettlement;
			private GeneratorManagement.StructureGeneration generation;

			public override void InitDraw(Transform parent, PlayerLocationManagement.DrawnLocation drawnLocation)
			{
				generation = new GeneratorManagement.StructureGeneration();

				generation.parent = parent;

				generation.SpawnStructure(GeneratorManagement.POOL_INDEXES.SETTLEMENT, Vector3.zero);
				//Apply simulation context to object
				LinkToBehaviour(generation.targets[^1].Item2);

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
				return 300.0f;
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

			public override QuestGiverData GetQuestGiver()
			{
				return questGiver;
			}

			public override bool CanBuyFuel()
			{
				return true;
			}

			public override List<StandardSimWeaponProfile> GetWeapons()
			{
				List<StandardSimWeaponProfile> baseList = base.GetWeapons();
				baseList.Add(weapon);
				return baseList;
			}

			public override void OnDeath()
			{
				if(actualSettlement.parent.Get().GetData(DataTags.Settlements, out SettlementsData setData))
				{
					setData.settlements.Remove(actualSettlement.setCellCenter);
				}
			}
		}

        public Settlement(RealSpacePosition pos, EntityLink parent)
        {
            actualSettlementPos = pos;
			//Set parent
			this.parent = parent;

			settlementRefinery = new RefineryData();
			settlementRefinery.parent = parent;
			settlementRefinery.refineryPosition = actualSettlementPos;

			location = new SettlementLocation();
            location.actualSettlement = this;
			location.shop = new Shop();
			//Limit settlement shops to only basic items, this is to stop players from just jumping around to different shops to try and find
			//a specific rare item.
			//That sort of gameplay sound really tedious and boring and I don't want to incentivis that
			location.shop.SetTargetRarity(ItemDatabase.ItemRarity.Basic);
			location.shop.capacity = 4;

			location.questGiver = new QuestGiverData();
			//Set quests origin to always be from this location
			location.questGiver.questOrigin = location;

			location.SetParent(parent);
			//Set shop to use same parent
			location.shop.parent = this.parent;
			location.questGiver.parent = this.parent;

			//Create weapon
			location.weapon = new SettlementWeapon();

			maxPop = SimulationManagement.random.Next(95, 106);
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
