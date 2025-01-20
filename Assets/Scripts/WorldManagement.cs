using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonitorBreak;
using System;
using UnityEngine.UIElements;

[IntializeAtRuntime]
public class WorldManagement : MonoBehaviour
{
    public const float inEngineWorldScaleMultiplier = 200f;
    public static readonly float invertedInEngineWorldScaleMultiplier = 1.0f / inEngineWorldScaleMultiplier;

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

    public static bool WithinValidSolarSystem(RealSpacePosition pos)
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

	public static RealSpacePosition RandomCellCenterWithinSolarSystem() 
	{
		RealSpacePosition toReturn = null;

		int loopClamp = 1000;
		do
		{
			if (loopClamp <= 0)
			{
				return null;
			}

			toReturn = new RealSpacePosition(
				LerpInSolarSystemRange(SimulationManagement.random.Next(-100, 101) / 100.0f),
				0,
				LerpInSolarSystemRange(SimulationManagement.random.Next(-100, 101) / 100.0f)
				);

			toReturn = ClampPositionToGrid(toReturn);

			if (!WithinValidSolarSystem(toReturn))
			{
				toReturn = null;
			}
			else if (Planet.availablePlanetPositions.Contains(toReturn))
			{
				toReturn = null;
			}

			loopClamp--;
		}
		while (toReturn == null);

		return toReturn;
	}

    private const double gridDensity = 3000;
	private static readonly double gridDensityHalf = gridDensity / 2;
	private static readonly int gridDensityIntHalf = (int)gridDensity/2;
	private static readonly int softGridDensityHalf = (int)((gridDensity / 2.0d) * 0.9d);

    public static double GetGridDensity()
    {
        return gridDensity;
    }

	public static double GetGridDensityHalf()
	{
		return gridDensityHalf;
	}

    public static RealSpacePosition ClampPositionToGrid(RealSpacePosition pos, double density = gridDensity)
    {
        return new RealSpacePosition(Math.Round(pos.x / density) * density, 0, Math.Round(pos.z / density) * density);
    }

    public static List<RealSpacePosition> GetNeighboursInGrid(RealSpacePosition origin)
    {
        List<RealSpacePosition> toReturn = new List<RealSpacePosition>();
        foreach (Vector3 offset in GenerationUtility.orthagonalOffsets)
        {
            toReturn.Add(new RealSpacePosition((offset.x * gridDensity) + origin.x, 0, (offset.z * gridDensity) + origin.z));
        }

        return toReturn;
    }

    public static RealSpacePosition RandomPositionInCell(RealSpacePosition chunkCenter, System.Random random)
    {
        return new RealSpacePosition(
            chunkCenter.x + random.Next(-softGridDensityHalf, softGridDensityHalf),
            chunkCenter.y,
            chunkCenter.z + random.Next(-softGridDensityHalf, softGridDensityHalf));
    }

    private const double debugMultiplier = 1000.0f;
    private const float debugSize = 2.0f;
    private static List<Vector3> debugPositions = new List<Vector3>();
    public static RealSpacePosition worldCenterPosition;

    public static void SetWorldCenterPosition(RealSpacePosition initialPos)
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

    public static RealSpacePosition OffsetFromWorldCenter(RealSpacePosition position, Vector3 additionalOffset)
    {
        RealSpacePosition toReturn = new RealSpacePosition(position);

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
public class RealSpacePosition
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

        RealSpacePosition otherPos = (RealSpacePosition)obj;

        return x == otherPos.x && y == otherPos.y && z == otherPos.z;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y, z);
    }

	public override string ToString()
	{
		return $"{x}, {y}, {z}";
	}

	//CONSTRUCTORS
	public RealSpacePosition(RealSpacePosition rsp)
    {
        x = rsp.x;
        y = rsp.y;
        z = rsp.z;
    }

    public RealSpacePosition(double x, double y, double z)
    {
        this.x = x; 
        this.y = y; 
        this.z = z;
    }

    public RealSpacePosition(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

	//OPERATIONS
	public RealSpacePosition Add(RealSpacePosition postion)
    {
        x += postion.x;
        y += postion.y;
        z += postion.z;

        return this;
    }

	public RealSpacePosition AddToClone(RealSpacePosition position)
	{
		return new RealSpacePosition(
			position.x + x,
			position.y + y,
			position.z + z
			);
	}

	public RealSpacePosition Divide(float value)
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

	public RealSpacePosition AddToClone(Vector3 value)
	{
		return new RealSpacePosition(
			value.x + x,
			value.y + y,
			value.z + z
			);
	}

	public RealSpacePosition Subtract(Vector3 value)
    {
        x -= value.x;
        y -= value.y;
        z -= value.z;

		return this;
    }

    public RealSpacePosition Subtract(RealSpacePosition value)
    {
        x -= value.x;
        y -= value.y;
        z -= value.z;

        return this;
    }

    public RealSpacePosition SubtractToClone(RealSpacePosition value)
    {
        return new RealSpacePosition(
            x - value.x,
            y - value.y,
            z - value.z
        );
    }

    //HELPER FUNCTIONS
    public double Magnitude()
    {
        double value = (x * x) + (y * y) + (z * z);
        return System.Math.Sqrt(value);
    }

    public double Distance(RealSpacePosition value)
    {
        return SubtractToClone(value).Magnitude();
    }

    public Vector3 AsVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }

    public RealSpacePosition Normalized()
    {
        double mag = Magnitude();
        return new RealSpacePosition(x / mag, y / mag, z / mag);
    }

    public Vector3 AsTruncatedVector3(double modifier)
    {
        return new Vector3((float)(x/modifier), (float)(y / modifier), (float)(z / modifier));
    }

    public Vector2 AsTruncatedVector2(double modifier)
    {
        return new Vector2((float)(x/modifier), (float)(z/modifier));
    }

	public static RealSpacePosition Lerp(RealSpacePosition a, RealSpacePosition b, float t)
	{
		return new RealSpacePosition(
			(a.x + (b.x - a.x) * t),
			(a.y + (b.y - a.y) * t),
			(a.z + (b.z - a.z) * t)
			); 
	}

	public RealSpacePosition Clone()
	{
		return new RealSpacePosition(x, y, z);
	}
}
