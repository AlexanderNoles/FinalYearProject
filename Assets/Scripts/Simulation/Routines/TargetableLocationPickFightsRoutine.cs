using EntityAndDataDescriptor;
using System.Collections.Generic;

[SimulationManagement.SimulationRoutine(40)]
public class TargetableLocationPickFightsRoutine : RoutineBase
{
	//Each tick this routine will cause targetable locations to try and pick fights
	//with other entites
	//This is based on the simulation principle of "entities working in union for the benefit of the player"
	//it doesn't make a lot of logical sense for a mineral depoist to be like "hey come kill me" but it does make
	//sense that if we think of entites as trying to create a believable world for the player

	public override void Run()
	{
		List<DataBase> targetableLocations = SimulationManagement.GetDataViaTag(DataTags.TargetableLocation);
	}
}