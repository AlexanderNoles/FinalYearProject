using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathHelper
{
    public interface IPath
    {
        public Vector3 GetPosition(float t);
        public Vector3 GetDirection(float t);
    }

    
    [System.Serializable]
    public struct Range
    {
        public float min;
        public float max;

        public float Get()
        {
            return Random.Range(min, max);
        }

        public Range(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }

    //SIMPLE PATH

    public class SimplePath : IPath
    {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;

        public Vector3 GetPosition(float t)
        {
            t = Mathf.Clamp01(t);
            float inverseT = 1.0f - t;

            return 
            (inverseT * ((inverseT * p0) + (t * p1))) +
            (t * ((inverseT * p1) + (t * p2)));
        }

        public Vector3 GetDirection(float t)
        {
            t = Mathf.Clamp01(t);
            float inverseT = 1.0f - t;

            return (((2 * inverseT) * (p1 - p0)) + ((2 * t) * (p2 - p1))).normalized;
        }

        public Quaternion GetRotation(float t)
        {
            return Quaternion.LookRotation(GetDirection(t), Vector3.up);
        }

        public void DrawDebug(Color color) 
        {
            Debug.DrawLine(p0, p1, color);
            Debug.DrawLine(p1, p2, color);
        }

        public void GenerateP1(Vector3 offset, bool enforcePositiveY = true)
        {
            //Find difference between p0 and p2
            Vector3 diff = (p2 - p0) * 0.5f;
            //Move up to the higher height
            diff.y *= 2.0f;

            if (enforcePositiveY)
            {
                diff.y = Mathf.Abs(diff.y);
            }

            p1 = p0 + diff + offset;
        }

        public void GenerateP1Straight()
        {
            Vector3 diff = (p2 - p0) * 0.5f;

            p1 = p0 + diff;
        }

        public float EstimateLength()
        {
            return Vector3.Distance(p0, p1) + Vector3.Distance(p1, p2);
        }
    }

    [System.Serializable]
    public class SimplePathParameters
    {
        public Vector3 rightVector = Vector3.right;
        public Vector3 forwardVector = Vector3.forward;
        public Range centerPointRange = new Range(0.4f, 0.6f);
        public Range pointOffsetRange = new Range(0.8f, 1.2f);
        public float pointOffsetMultiplier = 1.0f;
    }

    public static SimplePath GenerateSimplePath(Vector3 startPos, Vector3 endPos, SimplePathParameters pathParameters)
    {
        SimplePath toReturn = new SimplePath();
        toReturn.p0 = startPos;
        toReturn.p2 = endPos;

        //Calculate the control point (p1)
        //First linearly interpolate between p0 and p2 and then offset that by some right and forward value
        Vector3 p1 = Vector3.Lerp(startPos, endPos, pathParameters.centerPointRange.Get());
        
        float rightMultiplier = pathParameters.pointOffsetMultiplier * (Random.Range(0.0f, 1.0f) > 0.5f ? 1.0f : -1.0f);
        float forwardMultiplier = pathParameters.pointOffsetMultiplier * (Random.Range(0.0f, 1.0f) > 0.5f ? 1.0f : -1.0f);
        Vector3 offset = 
        (pathParameters.pointOffsetRange.Get() * rightMultiplier * pathParameters.rightVector) + 
        (pathParameters.pointOffsetRange.Get() * forwardMultiplier * pathParameters.forwardVector);

        toReturn.p1 = p1 + offset;

        return toReturn;
    }

    public static SimplePath GenerateSimplePathStatic(Vector3 startPos, Vector3 endPos, SimplePathParameters pathParameters)
    {
        SimplePath toReturn = new SimplePath();
        toReturn.p0 = startPos;
        toReturn.p2 = endPos;

        //Calculate the control point (p1)
        //First linearly interpolate between p0 and p2 and then offset that by some right and forward value
        Vector3 p1 = Vector3.Lerp(startPos, endPos, 0.5f);
        Vector3 offset =
        (pathParameters.pointOffsetMultiplier * pathParameters.rightVector) +
        (pathParameters.pointOffsetMultiplier * pathParameters.forwardVector);

        toReturn.p1 = p1 + offset;

        return toReturn;
    }

    public static bool ObjectBlockingPath(IPath path, LayerMask mask, out RaycastHit hitPos, int resolution = 10, bool accountForStartInWall = true)
    {
        float perMove = 1.0f / resolution;

        for (int i = 0; i < resolution; i++)
        {
            Vector3 startPos = path.GetPosition(i * perMove);

            if (accountForStartInWall && i == 0)
            {
                //Shift first check slightly backwards so we can avoid the path starting inside the wall
                startPos -= path.GetDirection(0.0f) * 0.01f;
            }

            Vector3 difference = path.GetPosition((i + 1) * perMove) - startPos;
            if (Physics.Raycast(startPos, difference, out RaycastHit hit, difference.magnitude, mask))
            {
                hitPos = hit;
                return true;
            }
        }

        hitPos = new RaycastHit();
        return false;
    }

    public static void DrawPath(IPath path, Color color, float time, int resolution = 10)
    {
        float perMove = 1.0f / resolution;

        for (int i = 0; i < resolution; i++)
        {
            Vector3 startPos = path.GetPosition(i * perMove);
            Vector3 difference = path.GetPosition((i + 1) * perMove) - startPos;

            Debug.DrawRay(startPos, difference, color, time);
        }
    }
}
