using EntityAndDataDescriptor;

[SimulationManagement.SimulationRoutine(-600)]
public class CalamityInitiateRoutine : RoutineBase
{
	public override void Run()
	{
		//Get calamity data
		GameWorld.main.GetData(DataTags.Calamity, out CalamityData calamityData);

		if (SimulationManagement.GetEntityCount(EntityTypeTags.Player) > 0)
		{
			int currentRandomTick = SimulationManagement.GetSimulationSeed() + SimulationManagement.currentTickID;
			
			//At regular intervals have something dramatic happen to the world
			//This means over time the world will get increasingly dramatic!
			if (calamityData.lastCalamityTick + calamityData.timeBetweenCalamities < currentRandomTick)
			{
				calamityData.lastCalamityTick = currentRandomTick;

				MonitorBreak.Bebug.Console.Log("Major Event Occured!");

				//Create some calamity
				//Spawn new void swarm
				//Currently only one choice for calamity so we just do that
				new VoidSwarm().Simulate();
			}
		}
	}
}