
public class TimerData : DataModule
{
	public int endTick;

	public TimerData(int length)
	{
		endTick = length + SimulationManagement.currentTickID;
	}
}