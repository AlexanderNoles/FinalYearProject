using EntityAndDataDescriptor;
using System;
using System.Collections.Generic;
using System.Linq;

[SimulationManagement.SimulationRoutine(3, SimulationManagement.SimulationRoutine.RoutineTypes.Init)]
public class NameInitRoutine : InitRoutineBase
{
	public override bool IsDataToInit(HashSet<Enum> tags)
	{
		return tags.Contains(DataTags.Name);
	}

	public override void Run()
	{
		//Get all name datas
		List<DataModule> dataModules = SimulationManagement.GetToInitData(DataTags.Name);

		foreach (NameData module in dataModules.Cast<NameData>())
		{
			//Because name datas are always bespoke per entity, there's isn't much we can do here apart from implementing per NameData subclass
			module.Generate();
		}
	}
}