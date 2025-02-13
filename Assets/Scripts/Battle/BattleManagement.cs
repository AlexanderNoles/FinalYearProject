using EntityAndDataDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleManagement : MonoBehaviour
{
	//Event run after global reset is done
	public static UnityEvent postGlobalRefresh = new UnityEvent();

	//Data generated post global reset
	public class PostGlobalRefreshStats
	{
		public List<Vector3> positionsOfBBsThatFoundTargets = new List<Vector3>();
		public Dictionary<int, int> idToCount = new Dictionary<int, int>();
	}

	public static PostGlobalRefreshStats refreshStats;

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
	private const int explosionIndex = 1;

	private static BattleManagement instance;

	public struct BasicEffectData
	{
		public float endTime;
		public float startTime;
		public float length;

		public Vector3 startPos;
		public Vector3 endPos;

		public Transform target;

		public void Offset(Vector3 offset)
		{
			target.position += offset;

			startPos += offset;
			endPos += offset;
		}

		public bool Done()
		{
			return Time.time > endTime;
		}

		public BasicEffectData(Transform target, float length, Vector3 startPos, Vector3 endPos) 
		{
			startTime = Time.time;
			endTime = startTime + length;
			this.length = length;

			this.target = target;
			this.startPos = startPos;
			this.endPos = endPos;
		}
	}

    private List<BasicEffectData> currentBasicBeamEffects = new List<BasicEffectData>();
	private Func<BasicEffectData, float, int> basicBeamFunc;
	private List<BasicEffectData> currentExplosionEffects = new List<BasicEffectData>();
	private Dictionary<Transform, Material> expTransformToMaterial = new Dictionary<Transform, Material>();
    private Func<BasicEffectData, float, int> explosionEffectFunc;

	public AnimationCurve explosionAnimCurve;
	private int nextGlobalRefresh;
	private float bbTickTime;

    private void Awake()
	{
		instance = this;
		postGlobalRefresh.RemoveAllListeners();
		refreshStats = new PostGlobalRefreshStats();

		basicBeamFunc = new Func<BasicEffectData, float, int>((BasicEffectData effectData, float percentage) =>
		{
			const float scale = 0.25f;

			//Find halfway point
			Vector3 displacement = Vector3.Lerp(effectData.startPos, effectData.endPos, percentage) - effectData.startPos;
			Vector3 halfway = effectData.startPos + (displacement / 2.0f);

			effectData.target.position = halfway;

            effectData.target.localScale =
				new Vector3(displacement.magnitude,
				percentage * scale,
				percentage * scale);

            return 0;
		});

		explosionEffectFunc = new Func<BasicEffectData, float, int>((BasicEffectData effectData, float percentage) =>
        {
			//Double percentage as half the effect should be spent with the fade out
			effectData.target.localScale = Vector3.one * (20 * explosionAnimCurve.Evaluate(percentage * 2.0f));

            return 0;
        });
    }

	private void Start()
	{	
		//Register for on location reset event
		PlayerCapitalShip.onPositionReset.AddListener(OnPositionReset);
	}

	private void OnDisable()
	{
		//Deregister from on location reset event
		PlayerCapitalShip.onPositionReset.RemoveListener(OnPositionReset);
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

	private void OnPositionReset()
	{
		Vector3 offset = -PlayerCapitalShip.GetPosBeforeReset();

		OffsetAll(currentBasicBeamEffects, offset);
		OffsetAll(currentExplosionEffects, offset);
    }

	private void OffsetAll(List<BasicEffectData> target, Vector3 offset)
	{
		foreach (BasicEffectData basicEffectData in target)
		{
			basicEffectData.Offset(offset);
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
		Transform targetBeam = attackEffectsPool.SpawnObject(basicBeamIndex, start).transform;
		targetBeam.LookAt(end);
		targetBeam.Rotate(0, -90, 0);
		targetBeam.localScale = Vector3.zero;

		currentBasicBeamEffects.Add(new BasicEffectData(targetBeam, length, start, end));
	}

	public static void CreateExplosion(Vector3 worldPos, float length) 
	{
        if (instance == null)
        {
            return;
        }

		instance.InitExplosion(worldPos, length);
    }

	private void InitExplosion(Vector3 pos, float length)
	{
		Transform targetExplosion = attackEffectsPool.SpawnObject(explosionIndex, pos).transform;

		if (!expTransformToMaterial.ContainsKey(targetExplosion))
		{
			expTransformToMaterial.Add(targetExplosion, targetExplosion.GetComponent<MeshRenderer>().material);
		}

		expTransformToMaterial[targetExplosion].SetFloat("_StartTime", Time.time);
		expTransformToMaterial[targetExplosion].SetFloat("_Length", length);

		BasicEffectData effectData = new BasicEffectData(targetExplosion, length, pos, pos);
		effectData.endTime += length * 4; //Add additional length to the end time
        currentExplosionEffects.Add(effectData);
	}

	private void Update()
	{
		if (SimulationManagement.RunningHistory())
		{
			//Don't run while history is running
			return;
		}

		RunEffect(basicBeamFunc, currentBasicBeamEffects, basicBeamIndex);
		RunEffect(explosionEffectFunc, currentExplosionEffects, explosionIndex);

		//Global process on battle behaviours
		//For each battle behaviour
		if (Time.frameCount > nextGlobalRefresh)
		{
			//During the global refresh we want to generate some data so other routines can tell stuff about the current battle state
			refreshStats = new PostGlobalRefreshStats();

			//Every 100 to 200 frames refresh the targets
			//Randomized to reduce chance of syncing up with frame spikes
			//without having a complicated execution priority system
			nextGlobalRefresh = Time.frameCount + UnityEngine.Random.Range(100, 200);

			//Get relationship data
			Dictionary<int, FeelingsData> idToFeelingsData = SimulationManagement.GetEntityIDToData<FeelingsData>(DataTags.Feelings);
			//Get contact policy data
			Dictionary<int, ContactPolicyData> idToContactData = SimulationManagement.GetEntityIDToData<ContactPolicyData>(DataTags.ContactPolicy);

			foreach (BattleBehaviour bb in colliderToBattleBehaviour.Values)
			{
				if (SimObjectBehaviour.IsSimObj(bb.GetType()))
				{
					SimObjectBehaviour simObjectBehaviour = (SimObjectBehaviour)bb;

					int entityID = simObjectBehaviour.TryGetEntityID();

					//Add to refresh stats count for this entity
					if (!refreshStats.idToCount.ContainsKey(entityID))
					{
						refreshStats.idToCount.Add(entityID, 0);
					}

					//Increment count
					refreshStats.idToCount[entityID]++;

					//Process
					if (bb.autoFindTargets)
					{
						//First clear the targets, (unless they are maintained)
						bb.ClearNonMaintainedTargets();

						//Base on whether any target was found, not neccesarily added. (As a found target might already be a maintained target)
						bool foundAnyTargets = false;

						//Get feelings data for this bb
						if (entityID == -1 || !idToFeelingsData.ContainsKey(entityID))
						{
							continue;
						}

						FeelingsData feelingsData = idToFeelingsData[entityID];
						//Now iterate over all other bbs in the scene and get ones that we could be aggressive towards
						foreach (BattleBehaviour otherBB in colliderToBattleBehaviour.Values)
						{
							int otherID = -1;

							if (SimObjectBehaviour.IsSimObj(otherBB.GetType()))
							{
								otherID = (otherBB as SimObjectBehaviour).TryGetEntityID();
							}

							if (otherID == entityID)
							{
								//Can't attack our own ships!
								continue;
							}
							else if (
								otherID == -1 ||
								!feelingsData.idToFeelings.ContainsKey(otherID) ||
								(idToContactData.ContainsKey(otherID) && idToContactData[otherID].openlyHostile) ||
								(idToContactData.ContainsKey(entityID) && idToContactData[entityID].openlyHostile)
								)
							{
								//Add target if it doesn't have an entity, or we have no feelings about this entity
								//So by default things attack each other

								//Also add it if it is openly hostile or we are openly hostile
								bb.AddTarget(otherBB, false);
								foundAnyTargets = true;
							}
							else if (feelingsData.idToFeelings[otherID].inConflict || feelingsData.idToFeelings[otherID].favourability < BalanceManagement.oppositionThreshold)
							{
								//Hostile, whether in marked conflict or low favourability
								//(30/01/2025) in conflict should ideally be removed at some point
								bb.AddTarget(otherBB, false);
								foundAnyTargets = true;
							}
						}

						if (foundAnyTargets)
						{
							refreshStats.positionsOfBBsThatFoundTargets.Add(bb.transform.position);
						}
					}
				}
			}

			//Invoke post event
			postGlobalRefresh.Invoke();
		}

		//Per tick update
		bbTickTime -= Time.deltaTime * SimulationManagement.GetSimulationSpeed();

		if (bbTickTime <=  0.0f)
		{
			int ticksPassedSinceLastFrame = 1 + Mathf.FloorToInt(Mathf.Abs(bbTickTime) / 1.0f);

			bbTickTime += ticksPassedSinceLastFrame;

			foreach (BattleBehaviour bb in colliderToBattleBehaviour.Values)
			{
				bb.DoTicks(ticksPassedSinceLastFrame);
			}
		}
	}

	private void RunEffect(Func<BasicEffectData, float, int> effectFunc, List<BasicEffectData> effects, int returnIndex)
	{
		for (int i = 0; i < effects.Count;) 
		{
			if (effects[i].Done())
			{
				attackEffectsPool.ReturnObject(returnIndex, effects[i].target);
				effects.RemoveAt(i);
			}
			else
            {
                float percentage = Mathf.Clamp01((Time.time - effects[i].startTime) / (effects[i].length));

                effectFunc.Invoke(effects[i], percentage);
				i++;
            }
		}
	}
}
