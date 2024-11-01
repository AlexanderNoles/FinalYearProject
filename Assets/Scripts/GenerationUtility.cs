using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenerationUtility 
{
    public static readonly Vector3[] orthagonalOffsets = new Vector3[] {
        new Vector3(1,0,0),
        new Vector3(0,0,1),
        new Vector3(-1,0,0),
        new Vector3(0,0,-1)
    };

    public static Vector3 GetCirclePositionBasedOnPercentage(float percentage, float radius)
    {
        percentage = percentage * 2 * Mathf.PI;

        Vector3 toReturn = new Vector3(Mathf.Sin(percentage) * radius, 0, Mathf.Cos(percentage) * radius);

        return toReturn;
    }

    public static HashSet<RealSpacePostion> GenerateFalloffPositionSpread(RealSpacePostion origin, Vector2 minMaxFalloffPerStep, int maxSize, Func<RealSpacePostion, bool> validPositionEval, System.Random random)
    {
        List<(float, RealSpacePostion)> frontier = new List<(float, RealSpacePostion)>
        {
            (1.0f, origin) //Add start point to frontier
        };

        //Closed set that will be returned
        HashSet<RealSpacePostion> closed = new HashSet<RealSpacePostion>();

        while (closed.Count < maxSize && frontier.Count > 0)
        {
            //Get current frontier point
            int index = random.Next(0, frontier.Count);
            (float, RealSpacePostion) current = frontier[index];

            //Remove it from frontier
            frontier.RemoveAt(index);

            //Add to closed
            if (!closed.Contains(current.Item2))
            {
                closed.Add(current.Item2);

                //If this current position does not have any spread power skip adding neighbours
                if (current.Item1 > 0.0f)
                {
                    //Get neighbours from world management
                    //If this neighbour is already closed then we don't care about it
                    List<RealSpacePostion> neighbours = WorldManagement.GetNeighboursInGrid(current.Item2);

                    foreach (RealSpacePostion pos in neighbours)
                    {
                        if (!closed.Contains(pos) && validPositionEval.Invoke(pos))
                        {
                            frontier.Add((current.Item1 - Mathf.Lerp(minMaxFalloffPerStep.x, minMaxFalloffPerStep.y, random.Next(0, 101) / 100.0f), pos));
                        }
                    }
                }
            }
        }

        return closed;
    }
}
