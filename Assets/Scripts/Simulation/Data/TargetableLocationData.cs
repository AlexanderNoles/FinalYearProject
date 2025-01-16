using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetableLocationData : DataBase
{
	public class ActualLocation : VisitableLocation
	{
		public TargetableLocationData target;
		public RealSpacePostion actualPos;

		public override RealSpacePostion GetPosition()
		{
			return actualPos;
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
	public RealSpacePostion position = null;
	private string name;
	private string description;
	private Color mapColour;

	public TargetableLocationData(string name, string description, Color mapColour)
	{
		this.name = name;
		this.description = description;
		this.mapColour = mapColour;

		location = new ActualLocation(this);
	}
}
