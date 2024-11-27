using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : SurroundingObject
{
    private Material targetMat;

    protected override void Awake()
    {
		scale *= (WorldManagement.solarSystemScaleModifier / WorldManagement.referncePointSolarSystemScaleModifier);

        base.Awake();
        Vector3 pos = transform.position * WorldManagement.solarSystemScaleModifier;
        postion = new RealSpacePostion(pos.x, pos.y, pos.z);
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

    public override void SetObjectVisualScale(float scale)
    {
        transform.localScale = Vector3.one * (scale * 3);
        targetMat.SetFloat("_Radius", scale);
    }
}
