using EntityAndDataDescriptor;
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
			OnDeath(new TakenDamageResult());
		}

		//Add bb weapons
		List<StandardSimWeaponProfile> wps = target.GetWeapons();

		foreach (StandardSimWeaponProfile wp in wps)
		{
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

	public float firePointsFuzziness = 1.0f;
	public List<Vector3> firePoints = new List<Vector3>();
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

	protected override Vector3 GetFireFromPosition(Vector3 targetPos)
	{
		if (firePoints.Count == 0)
		{
			return base.GetFireFromPosition(targetPos);
		}

		//Find the smallest angle between the look direction and the displacement of the target position from the fire point
		Vector3 pos = transform.position;
		Vector3 right = transform.right;
		Vector3 forward = transform.forward;
		Vector3 up = transform.up;

		Vector3 firePos = pos;
		float currentMinimum = float.MaxValue;

		for (int i = 0; i < firePoints.Count; i++)
		{
			//Make fire point non-local
			Vector3 calculatedFirePoint =
				(right * firePoints[i].x) +
				(forward * firePoints[i].z) +
				(up * firePoints[i].y);

			Vector3 lookDirection;
			if (Mathf.Abs(firePoints[i].x) > Mathf.Abs(firePoints[i].z))
			{
				//X component is dominant
				lookDirection = right * (firePoints[i].x / Mathf.Abs(firePoints[i].x));
			}
			else
			{
				//Z component is dominamt
				lookDirection = forward * (firePoints[i].z / Mathf.Abs(firePoints[i].z));
			}

			//Make fire point non local
			Vector3 calculatedOrigin = calculatedFirePoint + pos;

			Vector3 displacement = targetPos - calculatedOrigin;

			//Angle between 
			float angle = Mathf.Abs(Vector3.Angle(lookDirection, displacement)) + Random.Range(-firePointsFuzziness, firePointsFuzziness);

			if (angle < currentMinimum)
			{
				currentMinimum = angle;
				firePos = calculatedOrigin;
			}
		}

		return firePos;
	}

	protected override TakenDamageResult TakeDamage(float rawDamageNumber, BattleBehaviour origin)
	{
		if ((origin is SimObjectBehaviour && PlayerSimObjBehaviour.IsPlayerSimObjectBehaviour(origin as SimObjectBehaviour)))
		{
			if (target != null)
			{
				//Adjust player reputation
				target.AdjustPlayerReputation(rawDamageNumber * -BalanceManagement.damageReputationRatio);

				//If target is a visitable location, then we need to start a battle here
				if (target is VisitableLocation && GameWorld.main.GetData(DataTags.GlobalBattle, out GlobalBattleData globalBattleData))
				{
					//Position
					RealSpacePosition pos = (target as VisitableLocation).GetPosition();

					if (!globalBattleData.BattleExists(pos))
					{
						globalBattleData.StartOrJoinBattle(WorldManagement.ClampPositionToGrid(pos), pos, PlayerManagement.GetTarget().id, target.GetEntityID(), false);
					}
				}
			}
		}

		return base.TakeDamage(rawDamageNumber, origin);
	}

	protected override void OnDeath(TakenDamageResult result)
	{
		if (Linked())
		{
			target.OnDeath();

			//Give kill reward if killed by player
			if (PlayerManagement.PlayerEntityExists() && (result.origin is SimObjectBehaviour && PlayerSimObjBehaviour.IsPlayerSimObjectBehaviour(result.origin as SimObjectBehaviour)))
			{
				PlayerManagement.GetInventory().AdjustCurrency(target.GetKillReward() * BalanceManagement.killWorthRatio);
			}
		}
	}

	//Editor helper methods

	[ContextMenu("Log Entity ID")]
	public void LogEntityID()
	{
		Debug.Log(TryGetEntityID());
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;

		Transform transform = GetComponent<Transform>();

		Vector3 pos = transform.position;
		Vector3 right = transform.right;
		Vector3 forward = transform.forward;
		Vector3 up = transform.up;

		foreach (Vector3 firePoint in firePoints)
		{
			//Make fire point relative to rotation
			Vector3 calculatedFirePoint =
				(right * firePoint.x) +
				(forward * firePoint.z) +
				(up * firePoint.y);

			//Make fire point non local
			Vector3 calculatedOrigin = calculatedFirePoint + pos;

			Gizmos.DrawWireSphere(calculatedOrigin, 0.5f);
		}
	}
}
