using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoutineHelper
{
    public static bool AnyContains(List<TerritoryData> data, RealSpacePostion pos)
    {
        foreach (TerritoryData entry in data)
        {
            if (entry.Contains(pos))
            {
                return true;
            }
        }

        return false;
    }
}
