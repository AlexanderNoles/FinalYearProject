using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleData : DataBase
{
	public class BattleReference
	{

	}

	public Dictionary<RealSpacePostion, BattleReference> ongoingBattles = new Dictionary<RealSpacePostion, BattleReference>();
}
