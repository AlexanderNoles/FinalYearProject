
public class CalamityData : DataModule
{
	public int lastCalamityTick;
	public int timeBetweenCalamities = SimulationManagement.MonthToTickNumberCount(6);
}