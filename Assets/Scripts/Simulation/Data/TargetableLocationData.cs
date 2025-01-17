using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetableLocationData : DataBase
{
	public class ActualLocation : VisitableLocation
	{
		public TargetableLocationData target;

		public override RealSpacePostion GetPosition()
		{
			return target.actualPosition;
		}

		public ActualLocation(TargetableLocationData target)
		{
			this.target = target;
		}

		public override string GetTitle()
		{
			return target.name;
		}

		public override string GetDescription()
		{
			return target.description;
		}

		public override Color GetMapColour()
		{
			return target.mapColour;
		}

		public override float GetEntryOffset()
		{
			return 100.0f;
		}
	}

	public ActualLocation location;
	public RealSpacePostion cellCenter = null;
	public RealSpacePostion actualPosition;
	private string name;
	private string description;
	private Color mapColour;

	public int desirability = 1;

	public TargetableLocationData(string name, string description, Color mapColour)
	{
		this.name = name;
		this.description = description;
		this.mapColour = mapColour;

		location = new ActualLocation(this);
	}
}
