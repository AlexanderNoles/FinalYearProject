using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : CelestialBody
{
    public override Transform GetInWorldParent()
    {
        return transform;
    }
}
