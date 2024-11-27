using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisitableLocation : Location
{
	public virtual void InitDraw()
	{

	}

	public virtual void Cleanup()
	{

	}

	public virtual void Draw()
	{

	}

	public virtual Vector3 GetEntryOffset()
	{
		return Vector3.zero;
	}
}
