using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleData : DataModule
{
	public Dictionary<RealSpacePosition, GlobalBattleData.Battle> positionToOngoingBattles = new Dictionary<RealSpacePosition, GlobalBattleData.Battle>();
}
