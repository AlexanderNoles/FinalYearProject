using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArbitraryLocation : VisitableLocation
{
	public ArbitraryLocation SetLocation(RealSpacePosition pos)
	{
		postion = pos;

		return this;
	}

	public RealSpacePosition postion;

	public override RealSpacePosition GetPosition()
	{
		return postion;
	}
}
