using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBehaviour : MonoBehaviour
{
	//This id should only really be used for testing
	//All bBehaviour lookups should be done through colliders
	private int bBehaviourID;

	[HideInInspector]
	public new Transform transform;

	protected List<WeaponProfile> weapons = new List<WeaponProfile>();
	protected List<BattleBehaviour> currentTargets = new List<BattleBehaviour>();
	protected Collider cachedCollider = null;

	protected virtual void Awake()
	{
		transform = base.transform;
		bBehaviourID = Random.Range(-100000, 100000);
		cachedCollider = GetComponent<BoxCollider>();
	}

	protected virtual void OnEnable()
	{
		if (cachedCollider != null)
		{
			BattleManagement.RegisterBattleBehaviour(cachedCollider, this);
		}
	}

	protected virtual void OnDisable()
	{
		BattleManagement.DeRegisterBattleBehaviour(cachedCollider);
	}

	protected void ToggleTarget(BattleBehaviour target)
	{
        if (!currentTargets.Remove(target))
        {
            //If target is not removed (i.e., it was not in the list)
			currentTargets.Add(target);
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

		currentTargets.Add(newTarget);
	}

	protected void RemoveTarget(BattleBehaviour target)
	{
		currentTargets.Remove(target);
	}

	public void ClearTargets()
	{
		currentTargets.Clear();
	}

	private class AttackProfile 
	{
		public int totalNumberOfAttacks;
		public int numberOfAttacksSoFar;

		public float damagePerAttack;

		public AttackProfile(int total, float damage)
		{
			totalNumberOfAttacks = total;
			numberOfAttacksSoFar = 0;

			damagePerAttack = damage;
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
				//This does mean attack damage is processed on a per process basis, rather than per attack
				//Practically this means very little, especially on high frame rates
				attackProfiles.Add(new AttackProfile(attackCount, weaponProfile.GetDamage()));

				//Mark weapon has having fired
				weaponProfile.MarkLastAttackTime(Time.time);
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

			//Preprocess
			PreTargetProcess(target);

			//Apply attacks to targets
			//For each weapon we apply this targets share of the attacks rounded up
			for (int a = 0; a < attackProfiles.Count; a++)
			{
				AttackProfile attack = attackProfiles[a];

				//Number of attacks from this weapon for this target
				//Rounded up so we always use all our attacks, even if some targets don't get hit
				int attackCap = Mathf.CeilToInt(attack.totalNumberOfAttacks / (float)count);
				while (attack.numberOfAttacksSoFar < attack.totalNumberOfAttacks && attackCap > 0)
				{
					//Apply damage
					target.TakeDamage(attack.damagePerAttack);

					//Draw Attack
					DrawAttack(target.GetTargetablePosition(), weapons[a]);

					//Reduce attack cap
					attackCap--;
					//Increment number of attacks so far
					attack.numberOfAttacksSoFar++;
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

	protected virtual void DrawAttack(Vector3 targetPos, WeaponProfile weaponProfile)
	{
		//Be default simply ask the battle management script to draw a line to the target positon from our fire position
		BattleManagement.CreateBasicBeamEffect(GetFireFromPosition(), targetPos, 0.1f);
	}

	protected virtual Vector3 GetTargetablePosition()
	{
		//Return a position within some range of the center transform
		//a.k.a a fuzzy position

		const float fuzziness = 0.75f;
		return transform.position + Random.onUnitSphere * fuzziness;
	}

	protected virtual Vector3 GetFireFromPosition()
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
