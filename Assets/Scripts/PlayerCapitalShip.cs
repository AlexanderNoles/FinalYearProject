using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCapitalShip : MonoBehaviour
{
	[HideInInspector]
	public new Transform transform;

	public AnimationCurve turnCurve;
	public AnimationCurve jumpCurve;

	private static PlayerCapitalShip instance;
	private RealSpacePostion pcsRSP;
	private static bool jumping;
	private static JumpStage jumpStage;
	private static VisitableLocation jumpTarget;
	private static RealSpacePostion jumpStart;
	private static Quaternion lookAtTargetRot;
	private static Quaternion startTurnRot;
	private static float jumpT;
	private static float rotateT;
	private float postJumpT;

	private static float jumpBuildupBuffer;
	private const float jumpBuildupMax = 10.0f;

	private const float jumpBaseLineSpeed = 500.0f;

	[MonitorBreak.Bebug.ConsoleCMD("SHIPSPEED")]
	public static void SetShipSpeedCMD(string newSpeed)
	{
		shipSpeedMultiplier = int.Parse(newSpeed);
	}

	private static float shipSpeedMultiplier = 1;
	private static float thisJumpSpeed;

	private enum JumpStage
	{
		InitialTurn,
		JumpBuildup,
		ActualJump,
		PostJump,
		Done
	}

	[Header("Effects")]
	public ParticleSystem jumpPs;
	public Transform arcaneRingsParent;
	private List<Transform> arcaneRings = new List<Transform>();
	private List<Vector3> endOfJumpCachedRingPositions = new List<Vector3>();
	public LineRenderer trail;
	private const float maxTrailLength = 100.0f;
	public GameObject fireEffect;
	public GameObject portal;
	public MeshRenderer pulseRenderer;
	private Material pulseMat;
	private float pulseT;
	public Transform piercer;

	public AnimationCurve backingBuildupCurve;
	public Transform backingEngineBuildup;
	public GameObject engineLine;

	public GameObject outerEffect;

	private void Awake()
	{
		instance = this;
		jumping = false;

		transform = base.transform;

		portal.SetActive(false);
		fireEffect.SetActive(false);
		engineLine.SetActive(false);
		outerEffect.SetActive(false);

		pulseMat = pulseRenderer.material;
		pulseMat.SetFloat("_T", 0.0f);


		piercer.localScale = Vector3.zero;

		//Get arcane rings
		foreach (Transform ring in arcaneRingsParent)
		{
			arcaneRings.Add(ring);
			ring.localScale = Vector3.zero;
		}
	}

	public static void UpdatePCSPosition(RealSpacePostion pos)
	{
		instance.pcsRSP = pos;
		Shader.SetGlobalVector("_PCSPosition", instance.pcsRSP.AsTruncatedVector3(20000));
	}

	public static RealSpacePostion GetPCSPosition()
	{
		return instance.pcsRSP;
	}

	public static void SetRealWorldPos(Vector3 pos)
	{
		instance.transform.position = pos;
	}

	public static void ModelLookAt(Vector3 pos)
	{
		pos.y = instance.transform.position.y;

		instance.transform.LookAt(pos);

		//Adjustment for current model
		instance.transform.Rotate(Vector3.down * 90, Space.Self);
	}

	public static void StartJump(VisitableLocation target)
	{
		if (jumping)
		{
			return;
		}

		if (target == null || PlayerLocationManagement.IsPlayerLocation(target))
		{
			Debug.LogWarning("Can't jump to same location");
			return;
		}

		jumping = true;
		jumpStage = JumpStage.InitialTurn;
		jumpTarget = target;

		//Clone the position
		jumpStart = new RealSpacePostion(WorldManagement.worldCenterPosition);

		//Setup rotation
		//Get difference between positions jumpBaseLineSpeed
		RealSpacePostion difference = new RealSpacePostion(target.GetPosition()).Subtract(instance.pcsRSP);
		thisJumpSpeed = (float)(jumpBaseLineSpeed / difference.Magnitude());

		Vector3 vectorDifference = difference.AsTruncatedVector3(20000);

		lookAtTargetRot = Quaternion.LookRotation(vectorDifference, Vector3.up);
		startTurnRot = instance.transform.rotation;

		//Setup control variables
		jumpT = 0.0f;
		rotateT = 0.0f;
		jumpBuildupBuffer = jumpBuildupMax;
	}

	private void Update()
	{
		if (InputManagement.GetKeyDown(KeyCode.R))
		{
			//Get a random settlement location to teleport to
			List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Settlements);
			SettlementData.Settlement newTarget = null;
			while (newTarget == null)
			{
				int targetIndex = Random.Range(0, factions.Count);
				if (factions[targetIndex].GetData(Faction.Tags.Settlements, out SettlementData data))
				{
					if (data.settlements.Count > 0)
					{
						newTarget = data.settlements.ElementAt(Random.Range(0, data.settlements.Count)).Value;
					}
				}
			}
			if (newTarget != null)
			{
				StartJump(newTarget.location);
			}
		}

		if (jumping)
		{
			if (jumpStage == JumpStage.InitialTurn)
			{
				if (rotateT < 1.0f)
				{
					rotateT += Time.deltaTime * 0.1f;
					float originalY = transform.rotation.eulerAngles.y;
					transform.rotation = Quaternion.Lerp(startTurnRot, lookAtTargetRot, turnCurve.Evaluate(rotateT));

					float difference = transform.rotation.eulerAngles.y - originalY;

					CameraManagement.AddRotation(new Vector2(0, difference));

					if (rotateT >= 1.0f)
					{
						portal.SetActive(true);

						//Move to next stage
						jumpStage++;
					}
				}
			}
			else if (jumpStage == JumpStage.JumpBuildup)
			{
				if (jumpBuildupBuffer > 0.0f)
				{
					jumpBuildupBuffer -= Time.deltaTime;
					float percentage = Mathf.Clamp01(jumpBuildupBuffer / jumpBuildupMax);

					for (int i = 0; i < arcaneRings.Count; i++)
					{
						float ringT = Mathf.Pow(Mathf.Pow( Mathf.Clamp01((1.0f - percentage) * 2.0f) * 2.0f, 2.0f), (i * 0.25f) + 1);

						arcaneRings[i].localPosition = Vector3.forward * (40.0f - ((5.0f - Mathf.Clamp01(jumpBuildupBuffer / jumpBuildupMax)) * i * 2f));
						arcaneRings[i].localScale = Vector3.one * (ringT + (0.1f * Mathf.Sin(Time.time * 250)));
					}

					float piercerLength = 500.0f;
					piercer.localPosition = new Vector3(0, 0, piercerLength);
					float horiScale = Mathf.Max(Mathf.Lerp(-1f, 1f, percentage), 0.0f);
					piercer.localScale = new Vector3(horiScale, piercerLength * 2.0f, horiScale);


					backingEngineBuildup.localScale = Vector3.one * ((2.0f + (Mathf.Sin(Time.time * 50.0f) * 0.1f)) * backingBuildupCurve.Evaluate((1.0f - percentage) * 1.2f));
				}
				else
				{
					PlayerLocationManagement.ForceUnloadCurrentLocation();

					fireEffect.SetActive(true);
					jumpPs.Play();
					portal.SetActive(false);

					engineLine.SetActive(true);
					outerEffect.SetActive(true);

					piercer.localScale = Vector3.zero;

					pulseT = 1.0f;

					//Setup trail
					trail.gameObject.SetActive(true);
					trail.SetPositions(new Vector3[2]
					{
						Vector3.zero,
						Vector3.zero
					});

					SimulationManagement.SimulationSpeed("10");
					SimulationManagement.ForceTick();

					jumpStage++;
				}
			}
			else if (jumpStage == JumpStage.ActualJump)
			{
				//Lerp the world center from current to target
				if (jumpT < 1.0f)
				{
					jumpT += Time.deltaTime * thisJumpSpeed * shipSpeedMultiplier;

					trail.SetPosition(1, new Vector3(0, 0, Mathf.Lerp(0, -maxTrailLength, jumpT * 15.0f)));

					WorldManagement.SetWorldCenterPosition(RealSpacePostion.Lerp(jumpStart, jumpTarget.GetPosition(), jumpCurve.Evaluate(jumpT)));
					UpdatePCSPosition(WorldManagement.worldCenterPosition);

					if (jumpT >= 1.0f)
					{
						//We have arrived
						SimulationManagement.SimulationSpeed("1");
						jumpStage++;
						endOfJumpCachedRingPositions.Clear();

						postJumpT = 0.0f;

						jumpPs.Stop();

						fireEffect.SetActive(false);

						backingEngineBuildup.localScale = Vector3.zero;
						engineLine.SetActive(false);
						outerEffect.SetActive(false);

						pulseT = 1.0f;

						//currently just ending jump here
						EndJump();
					}

					for (int i = 0; i < arcaneRings.Count; i++)
					{
						Vector3 targetPosition = Vector3.back * (4.0f + ((((i + 1) * (jumpT + 1.0f) * 3.0f) + (0.1f * Mathf.Sin(Time.time * 500.0f))) * 5.0f));

						arcaneRings[i].localPosition = Vector3.Lerp(arcaneRings[i].localPosition, targetPosition, 10.0f * Time.deltaTime);

						if (jumpT >= 1.0f)
						{
							endOfJumpCachedRingPositions.Add(arcaneRings[i].localPosition);
						}
					}
				}
			}
		}

		if (jumpStage == JumpStage.PostJump)
		{
			postJumpT += Time.deltaTime;

			trail.SetPosition(0, new Vector3(0, 0, Mathf.Lerp(0, -maxTrailLength, postJumpT)));

			for (int i = 0; i < arcaneRings.Count; i++)
			{
				arcaneRings[i].localPosition = Vector3.Lerp(endOfJumpCachedRingPositions[i], Vector3.forward * 500.0f, postJumpT);
				arcaneRings[i].localScale = Vector3.Lerp(arcaneRings[i].localScale, new Vector3(1, 0, 1) * 1000.0f, Time.deltaTime * 2.0f);
			}

			GeneratorManagement.SetOffset(transform.forward * 500 * (1.0f - Mathf.Clamp01(postJumpT * 5f)));

			if (postJumpT >= 1.0f)
			{
				jumpStage++;
				trail.gameObject.SetActive(false);

				foreach (Transform ring in arcaneRings)
				{
					ring.localScale = Vector3.zero;
				}
			}
		}

		if (pulseT > 0.0f)
		{
			pulseT -= Time.deltaTime * 2.0f;

			pulseMat.SetFloat("_T", 1.0f - pulseT);
		}
	}

	private void EndJump()
	{
		jumping = false;

		//Update current player location
		PlayerLocationManagement.UpdateLocation(jumpTarget);

		//Set in engine position relative to current look direction (i.e., direction we entered location from)
		Vector3 newPosition = -transform.forward * jumpTarget.GetEntryOffset();

		transform.position = newPosition;
		//Make sure the camera doesn't just lerp to the new positions so it looks seamless
		CameraManagement.SetCameraPositionExternal(newPosition + CameraManagement.GetCameraDisplacementFromTarget(), true);
	}
}
