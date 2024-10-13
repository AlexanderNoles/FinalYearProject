using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : SurroundingObject
{
    private Material targetMat;

    protected override void Awake()
    {
        base.Awake();
        Vector3 pos = transform.position;

        float mag = pos.magnitude;
        mag -= 5.0f;

        //Fucked method to make sure orbits are cicular
        pos = Vector3.ClampMagnitude(pos, mag);
        
        pos *= 2000.0f;
        postion = new RealSpacePostion(pos.x, pos.y, pos.z);

        targetMat = GetComponent<MeshRenderer>().material;
        targetMat.SetVector("_RealSpacePosition", transform.position);
    }

    public override void SetObjectVisualScale(float scale)
    {
        transform.localScale = Vector3.one * (scale * 2);
        targetMat.SetFloat("_Radius", scale);
    }
}
