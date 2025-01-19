using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : SurroundingObject
{
    protected Material targetMat;

    public void Init()
    {
        scale *= (WorldManagement.solarSystemScaleModifier / WorldManagement.referncePointSolarSystemScaleModifier);
        Vector3 pos = transform.position * WorldManagement.solarSystemScaleModifier;
        postion = new RealSpacePosition(pos.x, pos.y, pos.z);
        //Clamp position to grid
        postion = WorldManagement.ClampPositionToGrid(postion);

        targetMat = GetComponent<MeshRenderer>().material;
        targetMat.SetVector("_RealSpacePosition", transform.position);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        WorldManagement.AddMajorWorldPart(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        WorldManagement.RemoveMajorWorldPart(this);
    }
}
