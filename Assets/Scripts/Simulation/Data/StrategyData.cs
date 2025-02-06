
using System.Collections.Generic;

public class StrategyData : DataModule
{
	public enum GlobalStrategy
	{
		Aggresive,
		Defensive
	}

	public GlobalStrategy globalStrategy = GlobalStrategy.Aggresive;
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
}