using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCenterLocation : VisitableLocation
{
    public override RealSpacePosition GetPosition()
    {
        return WorldManagement.worldCenterPosition;
    }
}
