using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisitableLocation : Location
{

	public virtual void Init()
	{

	}

	public virtual void Cleanup()
	{

	}

	public virtual void Draw()
	{

	}

	public virtual RealSpacePostion GetEntryPosition()
	{
		return GetPosition();
	}
}
