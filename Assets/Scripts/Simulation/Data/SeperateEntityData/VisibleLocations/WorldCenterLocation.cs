using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCenterLocation : VisitableLocation
{
    public override RealSpacePostion GetPosition()
    {
        return WorldManagement.worldCenterPosition;
    }
}
