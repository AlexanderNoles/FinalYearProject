using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurroundingObject : MonoBehaviour
{
    public float scale = 1f;
	public float shellOffset;
    protected float rawScale;
	[HideInInspector]
    public new Transform transform;
    public RealSpacePosition postion;

    protected virtual void Awake()
    {
        transform = base.transform;
        postion = new RealSpacePosition(0, 0, 0);
    }

    protected virtual void OnEnable()
    {
        SurroundingsRenderingManagement.RegisterSurroundingObject(this);
    }

    protected virtual void OnDisable()
    {
        SurroundingsRenderingManagement.DeRegisterSurroundingObject(this);
    }

    public virtual void SetObjectVisualScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }

	public void SetShellOffset(float newOffset)
	{
		shellOffset = newOffset;
	}

    public virtual void SetRawScale(float rawScale)
    {
        this.rawScale = rawScale;
    }

    public virtual Transform GetInWorldParent() 
    {
        return null;
    }
}
