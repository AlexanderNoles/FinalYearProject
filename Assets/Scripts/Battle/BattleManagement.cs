using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManagement : MonoBehaviour
{
	//Lookup battle behaviour system
	private static Dictionary<Collider, BattleBehaviour> colliderToBattleBehaviour = new Dictionary<Collider, BattleBehaviour>();

	public static void RegisterBattleBehaviour(Collider collider, BattleBehaviour target)
	{
		if (colliderToBattleBehaviour.ContainsKey(collider))
		{
			Debug.LogWarning("Collider is already registered!");
			return;
		}

		colliderToBattleBehaviour.Add(collider, target);
	}

	public static void DeRegisterBattleBehaviour(Collider collider)
	{
		colliderToBattleBehaviour.Remove(collider);
	}

	public static BattleBehaviour GetBattleBehaviour(Collider collider)
	{
		return colliderToBattleBehaviour[collider];
	}

	public static bool TryGetBattleBehaviour(Collider collider, out BattleBehaviour battleBehaviour)
	{
		if (colliderToBattleBehaviour.ContainsKey(collider))
		{
			battleBehaviour = colliderToBattleBehaviour[collider];
			return true;
		}

		battleBehaviour = null;
		return false;
	}
	//

	public MultiObjectPool attackEffectsPool;
	private const int basicBeamIndex = 0;

	private static BattleManagement instance;

	public struct BasicBeamData
	{
		public float endTime;
		public float startTime;
		public float length;

		public Transform target;
	}

	private List<BasicBeamData> currentBasicBeamEffects = new List<BasicBeamData>();

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		//Register for on location changed event
		PlayerLocationManagement.onLocationChanged.AddListener(OnLocationChanged);
		
		//Register for on location reset event
		PlayerCapitalShip.onPositionReset.AddListener(CleanupAllAttacks);
	}

	private void OnDisable()
	{
		//Deregister from on location changed event
		PlayerLocationManagement.onLocationChanged.RemoveListener(OnLocationChanged);

		//Deregister from on location reset event
		PlayerCapitalShip.onPositionReset.RemoveListener(CleanupAllAttacks);
	}

	private void OnLocationChanged()
	{
		CleanupAllAttacks();

		//Remove all targets for every battle behaviour
		foreach (KeyValuePair<Collider, BattleBehaviour> entry in colliderToBattleBehaviour)
		{
			entry.Value.ClearTargets();
		}
	}

	private void CleanupAllAttacks()
	{
		//Cleanup all attacks
		for (int i = 0; i < currentBasicBeamEffects.Count;)
		{
			attackEffectsPool.ReturnObject(basicBeamIndex, currentBasicBeamEffects[i].target);
			currentBasicBeamEffects.RemoveAt(i);
		}
	}

	public static void CreateBasicBeamEffect(Vector3 start, Vector3 end, float length)
	{
		if (instance == null)
		{
			return;
		}

		instance.InitBeamEffect(start, end, length);
	}

	private void InitBeamEffect(Vector3 start, Vector3 end, float length)
	{
		//Find halfway point
		Vector3 displacement = end - start;
		Vector3 halfWay = start + (displacement / 2);

		Transform targetBeam = attackEffectsPool.SpawnObject(basicBeamIndex, halfWay).transform;
		targetBeam.LookAt(end);
		targetBeam.Rotate(0, -90, 0);
		targetBeam.localScale = new Vector3(displacement.magnitude, 0.0f, 0.0f);

		BasicBeamData basicBeamData = new BasicBeamData();
		basicBeamData.startTime = Time.time;
		basicBeamData.endTime = Time.time + length;
		basicBeamData.length = length;
		basicBeamData.target = targetBeam;

		currentBasicBeamEffects.Add(basicBeamData);
	}

	private void Update()
	{
		for (int i = 0; i < currentBasicBeamEffects.Count;)
		{
			if (currentBasicBeamEffects[i].endTime <= Time.time)
			{
				attackEffectsPool.ReturnObject(basicBeamIndex, currentBasicBeamEffects[i].target);
				currentBasicBeamEffects.RemoveAt(i);
			}
			else
			{
				float percentage = Mathf.Clamp01((Time.time - currentBasicBeamEffects[i].startTime) / (currentBasicBeamEffects[i].length));

				currentBasicBeamEffects[i].target.localScale =
					new Vector3(currentBasicBeamEffects[i].target.localScale.x,
					percentage * 0.1f,
					percentage * 0.1f);

				i++;
			}
		}
	}
}
