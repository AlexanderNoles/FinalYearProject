
using EntityAndDataDescriptor;
using System.Collections.Generic;
using UnityEngine;

public class RetreatToReservesStrategy : StrategyData
{
	public override void OnBattleEnd(RealSpacePosition battlePos)
	{
		//If we have a military, send the ones at this position back home!
		if (TryGetLinkedData(DataTags.Military, out MilitaryData milData))
		{
			if (milData.positionToFleets.ContainsKey(battlePos))
			{
				int count = milData.positionToFleets[battlePos].Count;

				for (int i = 0; i < count; i++)
				{
					ShipCollection fleet = milData.RemoveFleet(battlePos);
					milData.AddFleetToReserves(fleet);
				}
			}
		}
	}
}