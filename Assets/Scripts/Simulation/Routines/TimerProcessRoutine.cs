using EntityAndDataDescriptor;
using System.Collections.Generic;
using System.Linq;

[SimulationManagement.SimulationRoutine(-2900)]
public class TimerProcessRoutine : RoutineBase
{
	public override void Run()
	{
		List<DataModule> timers = SimulationManagement.GetDataViaTag(DataTags.Timer);

		foreach (TimerData module in timers.Cast<TimerData>())
		{
			if (module.endTick < SimulationManagement.currentTickID)
			{
				//Kill parent
				module.parent.Get().AddTag(EntityStateTags.Dead);
			}
		}
	}
}