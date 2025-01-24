using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleContextLink : MonoBehaviour
{
	public virtual float GetMaxHealth()
	{
		return 100.0f;
	}

	public virtual void OnDeath()
	{

	}

	public virtual int GetEntityID()
	{
		return -1;
	}
}
