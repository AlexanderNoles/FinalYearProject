using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryData : DataBase
{
    public RealSpacePostion origin = null;
    public HashSet<RealSpacePostion> territoryCenters = new HashSet<RealSpacePostion>();
    public HashSet<RealSpacePostion> borders = new HashSet<RealSpacePostion>();
    public List<Vector3> borderInOrder = new List<Vector3>();

    public bool Contains(RealSpacePostion postion)
    {
        return territoryCenters.Contains(postion);
    }

	public void AddTerritory(RealSpacePostion center)
	{
		territoryCenters.Add(center);

		BorderCheck(center, true);
	}

	public void RemoveTerritory(RealSpacePostion center)
	{
		territoryCenters.Remove(center);

		BorderCheck(center, false);
	}

	private void BorderCheck(RealSpacePostion pos, bool hasCenter)
	{
		List<RealSpacePostion> toCheck = WorldManagement.GetNeighboursInGrid(pos);

		//If we have just removed this territory we don't bother checking whether it can be a border
		//if this check wasn't here kinda nothing would change but it feels cleaner this way
		if (hasCenter)
		{
			toCheck.Add(pos);
		}

		foreach (RealSpacePostion potentialBorder in toCheck)
		{
			//Couple of cases to account for:

			//First do we even own this territory...
			//...If we don't then we don't care
			if (territoryCenters.Contains(potentialBorder))
			{
				//Then we need to check if all neighbours for this are owned
				//If they are then we need to remove this from the border list
				//If they aren't then we need to add this
				//Assuming it isn't already removed or added in the first place

				bool allClaimed = true;
				List<RealSpacePostion> neighboursOfPotentialBorder = WorldManagement.GetNeighboursInGrid(potentialBorder);

				foreach (RealSpacePostion neighbourOfPB in neighboursOfPotentialBorder)
				{
					if (!territoryCenters.Contains(neighbourOfPB))
					{
						allClaimed = false;
						break; //Break check early
					}
				}

				if (!allClaimed)
				{
					//Should be a border
					if (!borders.Contains(potentialBorder))
					{
						borders.Add(potentialBorder);
					}
				}
				else
				{
					//Should not be a border
					//All already claimed
					if (borders.Contains(potentialBorder))
					{
						borders.Remove(potentialBorder);
					}
				}
			}
		}
	}
}
