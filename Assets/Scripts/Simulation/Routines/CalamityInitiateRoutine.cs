using EntityAndDataDescriptor;

[SimulationManagement.SimulationRoutine(-600)]
public class CalamityInitiateRoutine : RoutineBase
{
	//Gonna get rid of this routine at some point, to allow for more natural occurence of the calamites
	//So they can be a historical event as well

	//Maybe might add a version of this routine back in so I can calamities spawn at a more rapid pace once the player has shown up

	public override void Run()
	{
		return;

		//Get calamity data
		GameWorld.main.GetData(DataTags.Calamity, out CalamityData calamityData);

		int currentRandomTick = SimulationManagement.GetSimulationSeed() + SimulationManagement.currentTickID;

		//At regular intervals have something dramatic happen to the world
		//This means over time the world will get increasingly dramatic!
		if (calamityData.lastCalamityTick + calamityData.timeBetweenCalamities < currentRandomTick)
		{
			calamityData.lastCalamityTick = currentRandomTick;

			//Create some calamity

			if (SimulationManagement.random.Next(0, 101) < 30)
			{
				//Spawn new void swarm
				new VoidSwarm().Simulate();
			}
		}
	}
}