using System;
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
	private const int explosionIndex = 1;

    private static BattleManagement instance;

	public struct BasicEffectData
	{
		public float endTime;
		public float startTime;
		public float length;

		public Transform target;

		public void Offset(Vector3 offset)
		{
			target.position += offset;
		}

		public bool Done()
		{
			return Time.time > endTime;
		}

		public BasicEffectData(Transform target, float length) 
		{
			startTime = Time.time;
			endTime = startTime + length;
			this.length = length;

			this.target = target;
		}
	}

    private List<BasicEffectData> currentBasicBeamEffects = new List<BasicEffectData>();
	private Func<BasicEffectData, float, int> basicBeamFunc;
	private List<BasicEffectData> currentExplosionEffects = new List<BasicEffectData>();
	private Dictionary<Transform, Material> expTransformToMaterial = new Dictionary<Transform, Material>();
    private Func<BasicEffectData, float, int> explosionEffectFunc;

	public AnimationCurve explosionAnimCurve;

    private void Awake()
	{
		instance = this;

		basicBeamFunc = new Func<BasicEffectData, float, int>((BasicEffectData effectData, float percentage) =>
		{
            effectData.target.localScale =
				new Vector3(effectData.target.localScale.x,
				percentage * 0.1f,
				percentage * 0.1f);

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
		//Register for on location changed event
		PlayerLocationManagement.onLocationChanged.AddListener(OnLocationChanged);
		
		//Register for on location reset event
		PlayerCapitalShip.onPositionReset.AddListener(OnPositionReset);
	}

	private void OnDisable()
	{
		//Deregister from on location changed event
		PlayerLocationManagement.onLocationChanged.RemoveListener(OnLocationChanged);

		//Deregister from on location reset event
		PlayerCapitalShip.onPositionReset.RemoveListener(OnPositionReset);
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
		//Find halfway point
		Vector3 displacement = end - start;
		Vector3 halfWay = start + (displacement / 2);

		Transform targetBeam = attackEffectsPool.SpawnObject(basicBeamIndex, halfWay).transform;
		targetBeam.LookAt(end);
		targetBeam.Rotate(0, -90, 0);
		targetBeam.localScale = new Vector3(displacement.magnitude, 0.0f, 0.0f);

		currentBasicBeamEffects.Add(new BasicEffectData(targetBeam, length));
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

		BasicEffectData effectData = new BasicEffectData(targetExplosion, length);
		effectData.endTime += length * 4; //Add additional length to the end time
        currentExplosionEffects.Add(effectData);
	}

	private void Update()
	{
		RunEffect(basicBeamFunc, currentBasicBeamEffects, basicBeamIndex);
		RunEffect(explosionEffectFunc, currentExplosionEffects, explosionIndex);
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
