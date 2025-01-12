using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBehaviour : InteractableBase
{
	public string targetsName = "Target";
	public Collider targetCollider;
	//This id should only really be used for testing
	//All bBehaviour lookups should be done through colliders
	private int bBehaviourID;

	[HideInInspector]
	public new Transform transform;

	protected List<WeaponProfile> weapons = new List<WeaponProfile>();
	protected List<BattleBehaviour> currentTargets = new List<BattleBehaviour>();

    protected float currentHealth;

    protected virtual void Awake()
	{
		transform = base.transform;
		bBehaviourID = Random.Range(-100000, 100000);
	}

	protected void OnEnable()
	{
		BattleManagement.RegisterBattleBehaviour(targetCollider, this);
	}

	protected void OnDisable()
	{
		BattleManagement.DeRegisterBattleBehaviour(targetCollider);
	}

	protected void ToggleTarget(BattleBehaviour target)
	{
		//Can't attack ourself
		if (Equals(target))
		{
			return;
		}

        if (!RemoveTarget(target))
        {
            //If target is not removed (i.e., it was not in the list)
			AddTargetInternal(target);
        }
    }

	protected void AddTarget(BattleBehaviour newTarget)
	{
		//Can't attack ourself
		if (Equals(newTarget))
		{
			return;
		}

		//Can't target something twice
		if (currentTargets.Contains(newTarget))
		{
			return;
		}

		AddTargetInternal(newTarget);
	}

	private void AddTargetInternal(BattleBehaviour target)
	{
		if (currentTargets.Count == 0)
		{
			//No targets before this point
			foreach (WeaponProfile weapon in weapons)
			{
				weapon.OnBattleStart();
			}
		}

		OnAddTarget(target);
		currentTargets.Add(target);
	}

	protected bool RemoveTarget(BattleBehaviour target)
	{
		bool targetRemoved = currentTargets.Remove(target);

		if (targetRemoved)
		{
			OnRemoveTarget(target);
		}

		return targetRemoved;
	}

	public void ClearTargets()
	{
		OnClearTargets(currentTargets);
		currentTargets.Clear();
	}

	//Insert hook methods
	protected virtual void OnAddTarget(BattleBehaviour newTarget)
	{
		//Do nothing by default
	}

	protected virtual void OnRemoveTarget(BattleBehaviour target)
	{
		//Do nothing by default
	}

	protected virtual void OnClearTargets(List<BattleBehaviour> targetsBefore)
	{
		//Do nothing by default
	}
	//

	private class AttackProfile 
	{
		public WeaponProfile parentProfile;

		public int totalNumberOfAttacks;
		public int numberOfAttacksSoFar;

		public AttackProfile(int total, WeaponProfile weapon)
		{
			totalNumberOfAttacks = total;
			numberOfAttacksSoFar = 0;

			parentProfile = weapon;
		}
	}

	protected void ProcessTargets()
	{
		int count = currentTargets.Count;
		if(count <= 0)
		{
			//No targets
			return;
		}

		//Process attack pool so we know how many attacks we have to divide up between targets
		List<AttackProfile> attackProfiles = new List<AttackProfile>();

		foreach (WeaponProfile weaponProfile in weapons)
		{
			int attackCount = weaponProfile.CaclculateNumberOfAttacks();

			if (attackCount > 0)
			{
				attackProfiles.Add(new AttackProfile(attackCount, weaponProfile));
			}
		}

		//Apply damage to targets
		//Apply some random offset to the index so if too few attacks are applied on one frame
		//They don't always keep going to the first index, a.k.a single target fire
		int randomOffset = Random.Range(0, count);

		for (int i = 0; i < count; i++)
		{
			int index = (i + randomOffset) % count;
			BattleBehaviour target = currentTargets[index];

			Vector3 targetPosition = target.GetPosition();

			//Preprocess
			PreTargetProcess(target);

			//Apply attacks to target
			//For each weapon we apply this targets share of the attacks rounded up
			for (int a = 0; a < attackProfiles.Count; a++)
			{
				AttackProfile attack = attackProfiles[a];

				//Check if within range
				//If so then mark this weapon as used this frame
				if (Vector3.Distance(GetPosition(), targetPosition) <= attack.parentProfile.GetRange())
				{
					attack.parentProfile.MarkLastAttackTime(Time.time);

					//Number of attacks from this weapon for this target
					//Rounded up so we always use all our attacks, even if some targets don't get hit
					int attackCap = Mathf.CeilToInt(attack.totalNumberOfAttacks / (float)count);
					while (attack.numberOfAttacksSoFar < attack.totalNumberOfAttacks && attackCap > 0)
					{
						//Apply damage
						target.TakeDamage(attack.parentProfile.GetDamage());

						Vector3 shotTargetPosition = target.GetTargetablePosition();
						//Draw Attack
						DrawAttack(shotTargetPosition, GetFireFromPosition(shotTargetPosition), weapons[a]);

						//Reduce attack cap
						attackCap--;
						//Increment number of attacks so far
						attack.numberOfAttacksSoFar++;
					}
				}
			}

			//Postprocess
			PostTargetProcess(target);
		}
	}

	protected virtual void PreTargetProcess(BattleBehaviour target)
	{

	}

	protected virtual void PostTargetProcess(BattleBehaviour target)
	{

	}

	protected virtual void DrawAttack(Vector3 targetPos, Vector3 firePos, WeaponProfile weaponProfile)
	{
		//Be default simply ask the battle management script to draw a line to the target positon from our fire position
		BattleManagement.CreateBasicBeamEffect(firePos, targetPos, 0.1f);
		//Alongside a basic explosion
		//(Disabled: 27/12/2024)
		//BattleManagement.CreateExplosion(targetPos, 0.6f);
    }

	protected virtual Vector3 GetTargetablePosition()
	{
		//Return a position within some range of the center transform
		//a.k.a a fuzzy position

		const float fuzziness = 0.75f;
		return transform.position + Random.onUnitSphere * fuzziness;
	}

	protected virtual Vector3 GetFireFromPosition(Vector3 targetPos)
	{
		return transform.position;
	}

	protected virtual Vector3 GetPosition()
	{
		return transform.position;
	}

	//DAMAGE

	protected virtual void TakeDamage(float rawDamageNumber)
	{

	}

	//HELPER FUNCTIONS

	public Vector3 GetPointInsideCollider(BoxCollider collider)
	{
		Vector3 extents = collider.size / 2f;

		//Offset by collider center incase it isn't centered
		Vector3 point = new Vector3(
			Random.Range(-extents.x, extents.x),
			Random.Range(-extents.y, extents.y),
			Random.Range(-extents.z, extents.z)
		) + collider.center;

		return collider.transform.TransformPoint(point);
	}
}
