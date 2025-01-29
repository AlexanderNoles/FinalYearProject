using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : SurroundingObject
{
	public bool autoSetupRPS = true;
    protected Material targetMat;

    public virtual void Init(Vector3 pos)
    {
        scale *= (WorldManagement.solarSystemScaleModifier / WorldManagement.referncePointSolarSystemScaleModifier);
        position = new RealSpacePosition(
			(double)pos.x * WorldManagement.solarSystemScaleModifier, 
			(double)pos.y * WorldManagement.solarSystemScaleModifier, 
			(double)pos.z * WorldManagement.solarSystemScaleModifier
			);

        //Clamp position to grid
        position = WorldManagement.ClampPositionToGrid(position);

		targetMat = GetComponent<MeshRenderer>().material;

		if (autoSetupRPS)
		{
			targetMat.SetVector("_RealSpacePosition", transform.position);
		}

		WorldManagement.AddMajorWorldPart(this);
	}

	private void OnDestroy()
	{
		WorldManagement.RemoveMajorWorldPart(this);
	}
}
