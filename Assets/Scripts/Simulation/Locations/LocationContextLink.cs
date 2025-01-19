using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationContextLink : MonoBehaviour
{
	[HideInInspector]
	public VisitableLocation target;

	public void SetContext(VisitableLocation target)
	{
		this.target = target;
	}

	private void OnDisable()
	{
		//Clear context
		target = null;
	}
}
