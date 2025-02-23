using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryQuest : Quest
{
	public VisitableLocation target = null;

	public override string GetTitle()
	{
		if (target == null)
		{
			return "Delivery";
		}

		return $"Delivery to {target.GetTitle()}";
	}

	public override RealSpacePosition GetTargetPosition()
	{
		if (target == null)
		{
			return base.GetTargetPosition();
		}

		return target.GetPosition();
	}
}
