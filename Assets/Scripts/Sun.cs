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
			return;
		}

		float baseIntensity = WorldManagement.solarSystemScaleModifier * 2.0f;

		//Get the scale agnostic of shell offset
		float agnosticScale = scale / shellOffset;

		//This is not realistic but I think it looks cool :)
		float modifiedScale = Mathf.Pow(agnosticScale, 4.0f);
		//Invert so as sun gets bigger base intensity is divied by less
		float invertedScale = 1 / (modifiedScale / agnosticScale);

		dirLight.intensity = baseIntensity / invertedScale;
	}
}
