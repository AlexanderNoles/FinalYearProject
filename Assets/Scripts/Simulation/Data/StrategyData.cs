
using EntityAndDataDescriptor;
using System.Collections.Generic;

public class StrategyData : DataModule
{
	public enum GlobalStrategy
	{
		Aggresive,
		Defensive
	}

	public GlobalStrategy globalStrategy = GlobalStrategy.Aggresive;
	public bool removeTerritory = true;
	public float defensivePropensity = 3.0f;
	public float attackPositionUncertainty = 0.0f;

	public virtual List<int> GetTargets()
	{
		return new List<int>();
	}

	public virtual float GetDefensivePropensity()
	{
		return defensivePropensity;
	}

	public virtual void OnBattleEnd(RealSpacePosition battlePos)
	{
		//Any troops left in the battle position
		//Add them to the retreat buffer
		if (TryGetLinkedData(DataTags.Military, out MilitaryData milData))
		{
			if (milData.positionToFleets.ContainsKey(battlePos))
			{
				int count = milData.positionToFleets[battlePos].Count;

				for (int i = 0; i < count; i++)
				{
					ShipCollection fleet = milData.RemoveFleet(battlePos);

					if (fleet == null)
					{
						continue;
					}

					milData.AddFleetToRetreatBuffer(fleet);
				}
			}
		}
	}
}