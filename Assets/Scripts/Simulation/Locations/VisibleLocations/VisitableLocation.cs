using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VisitableLocation : Location
{
	//If we travel to this location, how far away do we want to arrive from?
	//This is relative to game loop scales (literally measured in engine world units), not the simulation's scales
	public virtual float GetEntryOffset()
	{
		return 100.0f;
	}

	//UI Display methods
	public override string GetDescription()
	{
		RealSpacePosition pos = GetPosition();

		return $"Coordinates: (X:{Math.Round(pos.x)}, Y:{Math.Round(pos.z)})";
	}
}
