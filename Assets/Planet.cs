using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : CelestialBody
{
    [Header("Planet Settings")]
    public Sun sun;

    public override Transform GetInWorldParent()
    {
        return sun.transform;
    }
}
