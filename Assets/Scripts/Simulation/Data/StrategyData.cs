
public class StrategyData : DataModule
{
	public enum GlobalStrategy
	{
		Aggresive,
		Defensive
	}

	public GlobalStrategy globalStrategy = GlobalStrategy.Aggresive;
	public float defensivePropensity = 3.0f;
}