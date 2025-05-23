using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetsGenerator : MonoBehaviour
{
    public Sun sun;
    public GameObject basePlanetPrefab;
    public GameObject baseMoonPrefab;

    [Header("Settings")]
    public AnimationCurve sizeCurve;

    public void GeneratePlanets(System.Random random, bool debug = false)
    {
		//3 to 5 planets, exclusive upper bound
		int planetNumber = random.Next(3, 6);

        if (debug)
        {
            planetNumber = 15;
        }

        const int overallPlanetSizeModifier = 1;
        const int lowerSizeBound = 350 * overallPlanetSizeModifier;
        const int upperSizeBound = 900 * overallPlanetSizeModifier;
        const int sizeDifference = upperSizeBound - lowerSizeBound;

        const int lowerPositionalBound = 1;
        const int upperPositionalBound = 25;
        const int positionalDifference = upperPositionalBound - lowerPositionalBound;

        float currentAngle = GenerateAngle(0, 360, random);
        float percentageVariance = 1.0f / planetNumber;
        percentageVariance *= 0.45f;

        if (debug)
        {
            percentageVariance = 0;
        }

        for (int i = 1; i <= planetNumber; i++)
        {
            //Calculate current angle
            if (debug)
            {
				currentAngle = 0.0f;//360 * (i / (float)planetNumber);
            }
            else
            {
                currentAngle = currentAngle + 180.0f;
                currentAngle = GenerateAngle(Mathf.RoundToInt(currentAngle - 15), Mathf.RoundToInt(currentAngle + 15), random);

                currentAngle = Mathf.Abs(currentAngle) % 360.0f;
            }

            Vector3 worldPos = GetPositionFromAngle(currentAngle);

            //Offset based on percentage through planets
            float percentage = (i / (float)planetNumber) + ((random.Next(-1000, 1001) / 1000.0f) * percentageVariance);
            worldPos *= lowerPositionalBound + (percentage * positionalDifference);

            Planet newPlanet = Instantiate(basePlanetPrefab, worldPos, Quaternion.identity, transform).GetComponent<Planet>();
            float relativeSize = sizeCurve.Evaluate((i - 1.0f) / planetNumber);
            newPlanet.scale = lowerSizeBound + (relativeSize * sizeDifference);
            newPlanet.sun = sun;

            newPlanet.Init(worldPos);
            bool hasRing = random.Next(0, 101) > 75;

            bool hasAtmosphere = percentage > 0.2 && percentage < 0.4;
            newPlanet.UpdatePlanetShader(random, relativeSize, percentage, hasAtmosphere);
            newPlanet.ActivateRing(hasRing);

            int numberOfMoons = random.Next(0, 4);
            int angleOffset = random.Next(0, 360);
            int angleOffsetPer = random.Next(75, 150);

            for (int m = 0; m < numberOfMoons; m++)
            {
                Vector3 moonPos = worldPos;
                //If close offset by m + 2
                moonPos += GetPositionFromAngle((angleOffset + (angleOffsetPer * m)) % 360.0f) * 
                    (m + 2.0f);

                Moon newMoon = Instantiate(baseMoonPrefab, moonPos, Quaternion.identity, transform).GetComponent<Moon>();
                newMoon.scale = (random.Next(30, 45) / 100.0f) * newPlanet.scale;
                newMoon.parent = newPlanet.transform;

                newMoon.Init(moonPos);
            }
        }
    }

    private float GenerateAngle(int min, int max, System.Random random, int resolution = 10000)
    {
        return random.Next(min * resolution, max * resolution) / (float)resolution;
    }

    private Vector3 GetPositionFromAngle(float angle)
    {
        return new Vector3(
            Mathf.Cos(angle),
            0,
            Mathf.Sin(angle)
            ).normalized;
    }
}
