using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : CelestialBody
{
    public static List<RealSpacePostion> availablePlanetPositions = new List<RealSpacePostion>();

    private void Start()
    {
        //Add position to planet list
        availablePlanetPositions.Add(WorldManagement.ClampPositionToGrid(postion));
    }

    [Header("Planet Settings")]
    public Sun sun;

    public override Transform GetInWorldParent()
    {
        return sun.transform;
    }
}
