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

		float baseIntensity = 2000.0f;

		//This is not realistic but I think it looks cool :)
		float modifiedScale = Mathf.Pow(scale, 2.0f);
		//Invert so as sun gets bigger base intensity is divied by less
		float invertedScale = 1 / (modifiedScale / this.scale);


		dirLight.intensity = baseIntensity / invertedScale;
	}
}
