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
}
