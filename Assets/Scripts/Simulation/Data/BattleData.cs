using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleData : DataBase
{
	public Dictionary<RealSpacePostion, GlobalBattleData.Battle> positionToOngoingBattles = new Dictionary<RealSpacePostion, GlobalBattleData.Battle>();
}
