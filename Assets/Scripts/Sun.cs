using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : CelestialBody
{
	public Light dirLight;

    protected void Awake()
    {
		//Auto init the sun
		Init(Vector3.zero);
    }

    public override Transform GetInWorldParent()
    {
        return transform;
    }

	private void Update()
	{
		//Update light intensity
		//Calculate worldcenter magnitude
		//Get the max radius of the solar system

		double lightPercentage = WorldManagement.worldCenterPosition.Magnitude() / WorldManagement.GetSolarSystemRadius();

		//Intensity at the edge of the solar system
		const float baseIntensity = 5.0f;
		Debug.Log(lightPercentage);
		Debug.Log(1.0f / (float)lightPercentage);
		dirLight.intensity = (1.0f / (float)lightPercentage) * baseIntensity;
	}
}
