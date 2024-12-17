using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapitalData : DataBase
{
	public class CapitalLocation : VisitableLocation
	{
		public CapitalData target;

		public override RealSpacePostion GetPosition()
		{
			return target.position;
		}

		public CapitalLocation(CapitalData target)
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
	}

	public CapitalLocation location;
	public RealSpacePostion position = null;
	private string name;
	private string description;
	private Color mapColour;

	public CapitalData(string name, string description, Color mapColour)
	{
		this.name = name;
		this.description = description;
		this.mapColour = mapColour;

		location = new CapitalLocation(this);
	}
}
