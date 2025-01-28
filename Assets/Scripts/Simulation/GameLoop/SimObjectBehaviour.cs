using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class that acts as the runtime representation side of sim objects
//Talks to gameloop systems, such as battle and interaction
public class SimObjectBehaviour : BoxDescribedBattleBehaviour
{
	public static bool IsSimObj(System.Type t)
	{
		return t.Equals(typeof(SimObjectBehaviour)) || t.IsSubclassOf(typeof(SimObjectBehaviour));
	}

	//Target control
	public SimObject target;

	public void Link(SimObject newTarget)
	{
		target = newTarget;

		OnLink();
	}

	public virtual void OnLink()
	{
		if (!Linked())
		{
			return;
		}

		currentHealth = target.GetMaxHealth();

		if (Dead())
		{
			OnDeath();
		}

		//Add bb weapons
		List<WeaponBase> wps = target.GetWeapons();

		foreach (WeaponBase wb in wps)
		{
			ContextLinkedWeaponProfile wp = new ContextLinkedWeaponProfile().SetTarget(wb);
			wp.MarkLastAttackTime(Time.time);
			weapons.Add(wp);
		}
	}

	public void UnLink()
	{
		target = null;
	}

	public bool Linked()
	{
		return target != null;
	}
	//

	//Helpers
	public void SetParent(Transform newParent)
	{
		transform.parent = newParent;
	}
	//

	//Battle Linking
	public virtual int TryGetEntityID()
	{
		if (!Linked())
		{
			return -1;
		}

		return target.GetEntityID();
	}

	public override float GetMaxHealth()
	{
		if (!Linked())
		{
			return base.GetMaxHealth();
		}

		return target.GetMaxHealth();
	}
	//

	[Header("Interaction Settings")]
	public bool canBeInteractedWith = true;
	public Collider mouseTargetCollider;

	protected override void OnEnable()
	{
		base.OnEnable();

		if (canBeInteractedWith)
		{
			PlayerInteractionManagement.AddInteractable(mouseTargetCollider, this);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		//Clear weapons
		weapons.Clear();

		//Remove from interactions system
		if (canBeInteractedWith)
		{
			PlayerInteractionManagement.RemoveInteractable(mouseTargetCollider);
		}

		UnLink();
	}

	protected override void OnDeath()
	{
		if (Linked())
		{
			target.OnDeath();
		}
	}

	//Editor helper methods

	[ContextMenu("Log Entity ID")]
	public void LogEntityID()
	{
		Debug.Log(TryGetEntityID());
	}
}
