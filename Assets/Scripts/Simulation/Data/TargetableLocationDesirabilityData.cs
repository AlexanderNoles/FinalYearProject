using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetableLocationDesirabilityData : DesirabilityData
{
	public TargetableLocationData target;

	public override RealSpacePosition GetCellCenter()
	{
		return target.cellCenter;
	}

	public override RealSpacePosition GetActualPosition()
	{
		return target.actualPosition;
	}
}
