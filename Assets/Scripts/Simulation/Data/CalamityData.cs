
public class CalamityData : DataModule
{
	public int lastCalamityTick;
	public int timeBetweenCalamities = SimulationManagement.YearsToTickNumberCount(10);
}