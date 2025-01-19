using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : CelestialBody
{
    [Header("Planet Visual Settings")]
    public GameObject ring;
    public Gradient possibleGroundColours;
    public AnimationCurve sizeToGroundColourIntensity;
    public Gradient heatToLandModifier;
    public Gradient possibleOceanColours;
    public AnimationCurve heatToOceanIntensity;

    public float waterThreshold = 0.6f;
    public Gradient nonWaterColours;

    [Header("Atmosphere Visual Settings")]
    public MeshRenderer atmosphere;
    public Vector3 lightWavelengths = new Vector3(700, 530, 440);
    public float scatteringStrength = 1;

    public static List<RealSpacePosition> availablePlanetPositions = new List<RealSpacePosition>();

    private void Start()
    {
        //Add position to planet list
        availablePlanetPositions.Add(WorldManagement.ClampPositionToGrid(postion));
    }

    public void ActivateRing(bool _bool)
    {
        ring.SetActive(_bool);

        if (_bool)
        {
            //Set ring look direction
            ring.transform.LookAt(ring.transform.position + transform.position.normalized);
            ring.transform.rotation *= Quaternion.Euler(0, 0, -15);
        }
    }

    public void UpdatePlanetShader(System.Random random, float relativeSizeT, float heatMagnitudeT, bool hasAtmosphere)
    {
        //Chose planet colours based on relative size and heat
        Color baseGroundColour = possibleGroundColours.Evaluate(random.Next(0, 10000) / 10000.0f);
        targetMat.SetColor("_LandColor", baseGroundColour * heatToLandModifier.Evaluate(heatMagnitudeT) * (sizeToGroundColourIntensity.Evaluate(relativeSizeT) * 2));

        float oceanIntensity = heatToOceanIntensity.Evaluate(heatMagnitudeT);
        targetMat.SetFloat("_LandMaskModifier", (oceanIntensity * 2.0f) - 1.0f);

        if (random.Next(0, 101) / 100.0f < waterThreshold) 
        {
            Color baseOceanColour = nonWaterColours.Evaluate(random.Next(0, 10000) / 10000.0f);
            targetMat.SetColor("_OceanColor", baseOceanColour);

            targetMat.SetFloat("_SpecularIntensity", 0.0f);
            targetMat.SetFloat("_OceanFlat", 0.0f);
        }
        else
        {
            Color baseOceanColour = possibleOceanColours.Evaluate(random.Next(0, 10000) / 10000.0f);
            targetMat.SetColor("_OceanColor", baseOceanColour);

            targetMat.SetFloat("_SpecularIntensity", 0.15f);
            targetMat.SetFloat("_OceanFlat", 1.0f);
        }

        if (hasAtmosphere)
        {
            atmosphere.gameObject.SetActive(true);
            //Atmosphere is handlded by a seperate object as it is transparent
            atmosphere.material.SetVector("_RealSpacePosition", transform.position);

            //Calculate our scattering coefficents
            float scatterR = Mathf.Pow(400 / lightWavelengths.x, 4) * scatteringStrength;
            float scatterG = Mathf.Pow(400 / lightWavelengths.y, 4) * scatteringStrength;
            float scatterB = Mathf.Pow(400 / lightWavelengths.z, 4) * scatteringStrength;

            //Pack them into a vector 3
            Vector3 scatteringCoefficents = new Vector3(scatterR, scatterG, scatterB);

            atmosphere.material.SetVector("_ScatteringCoefficents", scatteringCoefficents);
        }
        else
        {
            atmosphere.gameObject.SetActive(false);
        }
    }

    [Header("Planet Settings")]
    public Sun sun;

    public override Transform GetInWorldParent()
    {
        return sun.transform;
    }
}
