using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoutineBase : MonoBehaviour
{
    public virtual void Run()
    {

    }

    protected bool AnyContains(List<TerritoryData> data, RealSpacePostion pos)
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
