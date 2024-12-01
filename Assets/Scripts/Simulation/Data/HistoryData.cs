using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryData : DataBase
{
	public class HistoryCell
	{

	}

	public Dictionary<RealSpacePostion, HistoryCell> previouslyOwnedTerritories = new Dictionary<RealSpacePostion, HistoryCell>();
}
