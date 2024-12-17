using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak;
using System;

[IntializeAtRuntime]
public class WorldManagement : MonoBehaviour
{
    private static List<CelestialBody> majorWorldParts = new List<CelestialBody>();
    private static bool calculatedNewBounds = false;
    private static double solarSystemRadius = 0;
	private static readonly double lowerSolarSystemLimit = 5000;

	public static readonly float referncePointSolarSystemScaleModifier = 2000.0f;
	public static readonly float solarSystemScaleModifier = 2000.0f;

    public static void AddMajorWorldPart(CelestialBody newPart)
    {
        majorWorldParts.Add(newPart);
        calculatedNewBounds = false;
    }

    public static void RemoveMajorWorldPart(CelestialBody part)
    {
        majorWorldParts.Remove(part);
    }

    public static double GetSolarSystemRadius()
    {
        if (!calculatedNewBounds || solarSystemRadius == 0)
        {
            solarSystemRadius = 0;

            foreach (CelestialBody body in majorWorldParts)
            {
                double mag = body.postion.Magnitude();

                if (mag > solarSystemRadius)
                {
                    solarSystemRadius = mag;
                }
            }

            //Add some outer buffer
            solarSystemRadius += gridDensity * 2.0f;

            calculatedNewBounds = true;
        }

        return solarSystemRadius;
    }

    public static bool WithinValidSolarSystem(RealSpacePostion pos)
    {
        double mag = pos.Magnitude();

        //Limit positions to be within the solar system and not too close to the sun
        return mag < GetSolarSystemRadius() && mag > lowerSolarSystemLimit;
    }

	public static double ClampToSolarSystemRange(double input)
	{
		if (input < lowerSolarSystemLimit)
		{
			input = lowerSolarSystemLimit;
		}

		if (input > GetSolarSystemRadius())
		{
			input = GetSolarSystemRadius();
		}

		return input;
	}

	public static double LerpInSolarSystemRange(float t, bool clamp = true)
	{
		double difference = GetSolarSystemRadius() - lowerSolarSystemLimit;

		if (clamp)
		{
			t = Mathf.Clamp(t, -1.0f, 1.0f);
		}

		float absT = Mathf.Abs(t);
		float sign = t / absT;

		return sign * ((difference * absT) + lowerSolarSystemLimit);
	}

	public static RealSpacePostion RandomCellCenterWithinSolarSystem() 
	{
		RealSpacePostion toReturn = null;

		do
		{
			toReturn = new RealSpacePostion(
				LerpInSolarSystemRange(SimulationManagement.random.Next(-100, 101) / 100.0f),
				0,
				LerpInSolarSystemRange(SimulationManagement.random.Next(-100, 101) / 100.0f)
				);

			if (!WithinValidSolarSystem(toReturn))
			{
				toReturn = null;
			}
		}
		while (toReturn == null);

		return ClampPositionToGrid(toReturn);
	}

    private const double gridDensity = 3000;
	private static readonly double gridDensityHalf = gridDensity / 2;
	private static readonly int gridDensityIntHalf = (int)gridDensity/2;

    public static double GetGridDensity()
    {
        return gridDensity;
    }

	public static double GetGridDensityHalf()
	{
		return gridDensityHalf;
	}

    public static RealSpacePostion ClampPositionToGrid(RealSpacePostion pos)
    {
        return new RealSpacePostion(Math.Round(pos.x / gridDensity) * gridDensity, 0, Math.Round(pos.z / gridDensity) * gridDensity);
    }

    public static List<RealSpacePostion> GetNeighboursInGrid(RealSpacePostion origin)
    {
        List<RealSpacePostion> toReturn = new List<RealSpacePostion>();
        foreach (Vector3 offset in GenerationUtility.orthagonalOffsets)
        {
            toReturn.Add(new RealSpacePostion((offset.x * gridDensity) + origin.x, 0, (offset.z * gridDensity) + origin.z));
        }

        return toReturn;
    }

    public static RealSpacePostion RandomPositionInCell(RealSpacePostion chunkCenter, System.Random random)
    {
        return new RealSpacePostion(
            chunkCenter.x + random.Next(-gridDensityIntHalf, gridDensityIntHalf),
            chunkCenter.y,
            chunkCenter.z + random.Next(-gridDensityIntHalf, gridDensityIntHalf));
    }

    private const double debugMultiplier = 1000.0f;
    private const float debugSize = 2.0f;
    private static List<Vector3> debugPositions = new List<Vector3>();
    public static RealSpacePostion worldCenterPosition;

    public static void SetWorldCenterPosition(RealSpacePostion initialPos)
    {
        worldCenterPosition = initialPos;
    }

    public static void MoveWorldCenter(Vector3 offset)
    {
        worldCenterPosition.Add(offset);
    }

#if UNITY_EDITOR
    private void Update()
    {
        debugPositions.Clear();
    }
#endif

    //OPERATIONS

    public static RealSpacePostion OffsetFromWorldCenter(RealSpacePostion position, Vector3 additionalOffset)
    {
#if UNITY_EDITOR
        debugPositions.Add(position.AsTruncatedVector3(debugMultiplier));
#endif
        RealSpacePostion toReturn = new RealSpacePostion(position);

        return toReturn.Subtract(worldCenterPosition).Subtract(additionalOffset);
    }

    private void OnDrawGizmos()
    {
        Vector3 playerPos = worldCenterPosition.AsTruncatedVector3(debugMultiplier);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerPos, debugSize/2);

        Gizmos.color = Color.yellow;
        foreach (Vector3 position in debugPositions)
        {
            Gizmos.DrawWireSphere(position, debugSize);
        }
    }
}

[System.Serializable]
public class RealSpacePostion
{
    //Utilize doubles for extra precision
    //This position cannot be directly applied to a transform (beyond a certain point) as the loss of precision creates obvious errors
    public double x, y, z;

    //Custom equals so it works based off the position
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        RealSpacePostion otherPos = (RealSpacePostion)obj;

        return x == otherPos.x && y == otherPos.y && z == otherPos.z;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y, z);
    }

    //CONSTRUCTORS
    public RealSpacePostion(RealSpacePostion rsp)
    {
        x = rsp.x;
        y = rsp.y;
        z = rsp.z;
    }

    public RealSpacePostion(double x, double y, double z)
    {
        this.x = x; 
        this.y = y; 
        this.z = z;
    }

    public RealSpacePostion(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

	//OPERATIONS
	public RealSpacePostion Add(RealSpacePostion postion)
    {
        x += postion.x;
        y += postion.y;
        z += postion.z;

        return this;
    }

	public RealSpacePostion Divide(float value)
	{
		x /= value;
		y /= value;
		z /= value;

		return this;
	}

    public void Add(Vector3 value)
    {
        x += value.x;
        y += value.y;
        z += value.z;
    }

    public RealSpacePostion Subtract(Vector3 value)
    {
        x -= value.x;
        y -= value.y;
        z -= value.z;

		return this;
    }

    public RealSpacePostion Subtract(RealSpacePostion value)
    {
        x -= value.x;
        y -= value.y;
        z -= value.z;

        return this;
    }

    //HELPER FUNCTIONS
    public double Magnitude()
    {
        double value = (x * x) + (y * y) + (z * z);
        return System.Math.Sqrt(value);
    }

    public Vector3 AsVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }

    public RealSpacePostion Normalized()
    {
        double mag = Magnitude();
        return new RealSpacePostion(x / mag, y / mag, z / mag);
    }

    public Vector3 AsTruncatedVector3(double modifier)
    {
        return new Vector3((float)(x/modifier), (float)(y / modifier), (float)(z / modifier));
    }

	public static RealSpacePostion Lerp(RealSpacePostion a, RealSpacePostion b, float t)
	{
		return new RealSpacePostion(
			(a.x + (b.x - a.x) * t),
			(a.y + (b.y - a.y) * t),
			(a.z + (b.z - a.z) * t)
			); 
	}
}
