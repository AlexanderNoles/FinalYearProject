using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenerationUtility 
{
	public static readonly Vector3[] normalizedDiagonalOffsets = new Vector3[] {
		new Vector3(-1,0,1).normalized,
		new Vector3(-1,0,-1).normalized,
		new Vector3(1,0,-1).normalized,
		new Vector3(1,0,1).normalized
	};

	public static readonly Vector3[] diagonalOffsets = new Vector3[] {
		new Vector3(-1,0,1),
		new Vector3(-1,0,-1),
		new Vector3(1,0,-1),
		new Vector3(1,0,1)
	};

	public static readonly Vector2[] diagonalOffsets2D = new Vector2[] {
		new Vector2(1,1),
		new Vector2(1,-1),
		new Vector2(-1,-1),
		new Vector2(-1,1),
	};

	public static readonly Vector3[] orthagonalOffsets = new Vector3[] {
        new Vector3(1,0,0),
        new Vector3(0,0,1),
        new Vector3(-1,0,0),
        new Vector3(0,0,-1)
    };

    public static readonly Vector3[] omniDirectionalOffsets = new Vector3[] {
        new Vector3(1,0,1),
        new Vector3(-1,0,1),
        new Vector3(-1,0,-1),
        new Vector3(1,0,-1),
        new Vector3(1,0,0),
        new Vector3(0,0,1),
        new Vector3(-1,0,0),
        new Vector3(0,0,-1)
    };

	public static readonly Vector3[] nineDirectionalOffsets = new Vector3[] {
		new Vector3(1,0,1),
		new Vector3(-1,0,1),
		new Vector3(-1,0,-1),
		new Vector3(1,0,-1),
		Vector3.zero,
		new Vector3(1,0,0),
		new Vector3(0,0,1),
		new Vector3(-1,0,0),
		new Vector3(0,0,-1)
	};

	public static Vector3 GetCirclePositionBasedOnPercentage(float percentage, float radius)
    {
		return GetCirclePositionBasedOnPercentage(percentage, radius, Vector3.zero);
    }

	public static Vector3 GetCirclePositionBasedOnPercentage(float percentage, float radius, Vector3 center)
	{
		percentage = percentage * 2 * Mathf.PI;

		Vector3 toReturn = new Vector3(Mathf.Sin(percentage) * radius, 0, Mathf.Cos(percentage) * radius) + center;

		return toReturn;
	}

    public static HashSet<RealSpacePosition> GenerateFalloffPositionSpread(RealSpacePosition origin, Vector2 minMaxFalloffPerStep, int maxSize, Func<RealSpacePosition, bool> validPositionEval, System.Random random)
    {
        List<(float, RealSpacePosition)> frontier = new List<(float, RealSpacePosition)>
        {
            (1.0f, origin) //Add start point to frontier
        };

        //Closed set that will be returned
        HashSet<RealSpacePosition> closed = new HashSet<RealSpacePosition>();

        while (closed.Count < maxSize && frontier.Count > 0)
        {
            //Get current frontier point
            int index = random.Next(0, frontier.Count);
            (float, RealSpacePosition) current = frontier[index];

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
                    List<RealSpacePosition> neighbours = WorldManagement.GetNeighboursInGrid(current.Item2);

                    foreach (RealSpacePosition pos in neighbours)
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

    public static bool IsShapeSelfIntersecting(List<Vector3> shape, int backwardsLimit = 5)
    {
        Vector3 outStandIn = Vector3.zero;
        backwardsLimit = Mathf.Clamp(backwardsLimit, 0, shape.Count - 2);

        int limit = shape.Count - (backwardsLimit+1);
        for (int i = shape.Count - 1; i > limit && i > 4; i--)
        {
            if (DoLineSegmentsIntersect(
                shape[i], 
                shape[i-1],
                shape[i-2],
                shape[i-3],
                out outStandIn,
                0.0f
                ))
            {
                return true;
            }
        }

        return false;
    }

    public static bool DoLineSegmentsIntersect(Vector3 line1point1, Vector3 line1point2, Vector3 line2point1, Vector3 line2point2, out Vector3 intersectionPoint, float outputHeight = 0)
    {
        bool toReturn = DoLineSegmentsIntersect(
            new Vector2(line1point1.x, line1point1.z),
            new Vector2(line1point2.x, line1point2.z),
            new Vector2(line2point1.x, line2point1.z),
            new Vector2(line2point2.x, line2point2.z),
            out Vector2 intersectionPoint2D);

        intersectionPoint = new Vector3(intersectionPoint2D.x, outputHeight, intersectionPoint2D.y);

        return toReturn;
    }

    public static bool DoLineSegmentsIntersect(Vector2 line1point1, Vector2 line1point2, Vector2 line2point1, Vector2 line2point2, out Vector2 intersectionPoint)
    {
        Vector2 a = line1point2 - line1point1;
        Vector2 b = line2point1 - line2point2;
        Vector2 c = line1point1 - line2point1;

        float alphaNumerator = b.y * c.x - b.x * c.y;
        float betaNumerator = a.x * c.y - a.y * c.x;
        float denominator = a.y * b.x - a.x * b.y;

        intersectionPoint = new Vector2();

        if (denominator == 0)
        {
            return false;
        }
        else if (denominator > 0)
        {
            if (alphaNumerator < 0 || alphaNumerator > denominator || betaNumerator < 0 || betaNumerator > denominator)
            {
                return false;
            }
        }
        else if (alphaNumerator > 0 || alphaNumerator < denominator || betaNumerator > 0 || betaNumerator < denominator)
        {
            return false;
        }

        intersectionPoint.x = line1point1.x + (alphaNumerator * a.x) / denominator;
        intersectionPoint.y = line1point1.y + (alphaNumerator * a.y) / denominator;

        return true;
    }
}
