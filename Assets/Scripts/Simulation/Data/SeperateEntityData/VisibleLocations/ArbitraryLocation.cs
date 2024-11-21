using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArbitraryLocation : VisitableLocation
{
	public ArbitraryLocation SetLocation(RealSpacePostion pos)
	{
		postion = pos;

		return this;
	}

	public RealSpacePostion postion;

	public override RealSpacePostion GetPosition()
	{
		return postion;
	}
}
