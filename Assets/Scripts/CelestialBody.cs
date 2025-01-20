using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : SurroundingObject
{
    protected Material targetMat;

    public virtual void Init()
    {
        scale *= (WorldManagement.solarSystemScaleModifier / WorldManagement.referncePointSolarSystemScaleModifier);
        Vector3 pos = transform.position * WorldManagement.solarSystemScaleModifier;
        postion = new RealSpacePosition(pos.x, pos.y, pos.z);
        //Clamp position to grid
        postion = WorldManagement.ClampPositionToGrid(postion);

        targetMat = GetComponent<MeshRenderer>().material;
        targetMat.SetVector("_RealSpacePosition", transform.position);
		WorldManagement.AddMajorWorldPart(this);
	}

	private void OnDestroy()
	{
		WorldManagement.RemoveMajorWorldPart(this);
	}
}
