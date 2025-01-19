using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisitableLocationHealthContextLink : HealthContextLink
{
	private LocationContextLink locationLink;

	private void OnEnable()
	{
		//Get location contex link
		locationLink = GetComponent<LocationContextLink>();
	}

	public override float GetMaxHealth()
	{
		if (locationLink == null || locationLink.target == null || !locationLink.target.HasHealth())
		{
			return base.GetMaxHealth();
		}

		return locationLink.target.GetMaxHealth();
	}

	public override void OnDeath()
	{
		if (locationLink != null)
		{
			locationLink.target.OnDeath();
		}
	}
}
