using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthContextLink : MonoBehaviour
{
	public virtual float GetMaxHealth()
	{
		return 100.0f;
	}

	public virtual void OnDeath()
	{

	}
}
