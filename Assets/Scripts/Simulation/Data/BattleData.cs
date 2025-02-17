using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleData : DataModule
{
	public Dictionary<RealSpacePosition, GlobalBattleData.Battle> positionToOngoingBattles = new Dictionary<RealSpacePosition, GlobalBattleData.Battle>();

	public override string Read()
	{
		return $"	Battle Count: {positionToOngoingBattles.Count}";
	}

	[MonitorBreak.Bebug.ConsoleCMD("BATTLEINFO", "Get battle info for a entity")]
	public static void OutputBattleInfo(string entityID)
	{
		SimulationEntity simulationEntity = SimulationManagement.GetEntityByID(int.Parse(entityID));

		if (simulationEntity != null )
		{
			if (simulationEntity.GetData(DataTags.Battle, out BattleData bData))
			{
				MonitorBreak.Bebug.Console.Log(simulationEntity.id);

				foreach (KeyValuePair<RealSpacePosition, GlobalBattleData.Battle> entry in bData.positionToOngoingBattles)
				{
					MonitorBreak.Bebug.Console.Log($"	{entry.Key}");
				}
			}
		}
	}
}
