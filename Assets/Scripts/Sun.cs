using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : CelestialBody
{
	public Light dirLight;

    public override Transform GetInWorldParent()
    {
        return transform;
    }

	public override void SetObjectVisualScale(float scale)
	{
		base.SetObjectVisualScale(scale);

		if (shellOffset == -1)
		{
			//Shell based system is not currently being used
			//This means we are in the map
			return;
		}

		float baseIntensity = WorldManagement.solarSystemScaleModifier * 2.0f;

		//This is not realistic but I think it looks cool :)
		float modifiedScale = Mathf.Pow(rawScale, 4.0f);
		//Invert so as sun gets bigger base intensity is divied by less
		float invertedScale = 1 / (modifiedScale / rawScale);

		dirLight.intensity = baseIntensity / invertedScale;
	}
}
