using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : SurroundingObject
{
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
        targetMat.SetVector("_RealSpacePosition", transform.position);
		WorldManagement.AddMajorWorldPart(this);
	}

	private void OnDestroy()
	{
		WorldManagement.RemoveMajorWorldPart(this);
	}
}
