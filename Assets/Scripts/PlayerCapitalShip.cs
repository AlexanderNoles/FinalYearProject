using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCapitalShip : MonoBehaviour
{
	public static UnityEvent onPositionReset = new UnityEvent();

	[HideInInspector]
	public new Transform transform;

	public static Vector3 GetPosition()
	{
		return instance.transform.position;
	}

	public AnimationCurve turnCurve;
	public AnimationCurve jumpCurve;

	private static PlayerCapitalShip instance;
	private RealSpacePostion pcsRSP;
	private static bool jumping;

	public static bool IsJumping()
	{
		return jumping;
	}

	public static JumpStage CurrentStage()
	{
		return jumpStage;
	}

	private static JumpStage jumpStage;
	private static VisitableLocation jumpTarget;

	public static RealSpacePostion GetTargetPosition()
	{
		return jumpTarget.GetPosition();
	}

	public static bool IsTargetPosition(RealSpacePostion pos)
	{
		if (pos == null || jumpTarget == null)
		{
			return false;
		}

		return pos.Equals(GetTargetPosition());
	}

	private static RealSpacePostion jumpEnd;
	private static RealSpacePostion jumpStart;
	private static Quaternion lookAtTargetRot;
	private static Quaternion startTurnRot;
	private static float jumpT;
	private static float rotateT;
	private float postJumpT;
	private float fuelAtBeginningOfJump;
	private float fuelAtEndOfJump;
	private static float nextJumpAllowedTime;

	private static float jumpBuildupBuffer;
	private const float jumpBuildupMax = 10.0f;

	public const float jumpBaseLineSpeed = 500.0f;

	[MonitorBreak.Bebug.ConsoleCMD("SHIPSPEED")]
	public static void SetShipSpeedCMD(string newSpeed)
	{
		shipSpeedMultiplier = int.Parse(newSpeed);
	}

	private static float shipSpeedMultiplier = 1;
	private static float thisJumpSpeed;

	public enum JumpStage
	{
		InitialTurn,
		JumpBuildup,
		ActualJump,
		PostJump,
		Done
	}

	private float rotationalMovement;
	private float normalEnginesIntensity;
	private const float engineIntensityMax = 5.0f;

	private Vector3 lastRecordedPos;
	private static Vector3 posBeforeReset;

	public static Vector3 GetPosBeforeReset()
	{
		return posBeforeReset;
	}

	public Rigidbody rigidbodyTarget;

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
		lastRecordedPos = Vector3.zero;

		portal.SetActive(false);
		fireEffect.SetActive(false);
		engineLine.SetActive(false);
		outerEffect.SetActive(false);

		pulseMat = pulseRenderer.material;
		pulseMat.SetFloat("_T", 0.0f);

		//Set inital jump stage
		jumpStage = JumpStage.Done;

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

	public static Vector3 GetForward()
	{
		return instance.transform.forward;
	}

	public static void StartJump(VisitableLocation newJumpTarget)
	{
		if (jumping || nextJumpAllowedTime > Time.time)
		{
			return;
		}

		if (newJumpTarget == null || PlayerLocationManagement.IsPlayerLocation(newJumpTarget))
		{
			Debug.LogWarning("Can't jump to same location (or location is non-existant!)");
			return;
		}

		//By default don't change fuel during jump
		instance.fuelAtEndOfJump = -1;

		jumping = true;
		jumpStage = JumpStage.InitialTurn;
		jumpTarget = newJumpTarget;

		//Zero current rotational movement
		instance.rotationalMovement = 0;
		//Zero current engine intensity
		instance.normalEnginesIntensity = 0;
		instance.UpdateEngineIntensityVisuallyAuto();

		//Get start position
		jumpStart = new RealSpacePostion(WorldManagement.worldCenterPosition);

		//Get final position
		jumpEnd = new RealSpacePostion(jumpTarget.GetPosition());

		//Setup rotation
		//Get difference between positions jumpBaseLineSpeed
		RealSpacePostion difference = new RealSpacePostion(jumpEnd).Subtract(jumpStart);
		thisJumpSpeed = (float)(jumpBaseLineSpeed / difference.Magnitude());

		Vector3 vectorDifference = difference.AsTruncatedVector3(20000);

		lookAtTargetRot = Quaternion.LookRotation(vectorDifference, Vector3.up);
		startTurnRot = instance.transform.rotation;

		//Add entry offset to jump target
		jumpEnd.Subtract(jumpTarget.GetEntryOffset() * vectorDifference.normalized * WorldManagement.invertedInEngineWorldScaleMultiplier);

		//Setup control variables
		jumpT = 0.0f;
		rotateT = 0.0f;
		jumpBuildupBuffer = jumpBuildupMax;
	}

	public static void HaveFuelChangeOverJump(float startValue, float endValue)
	{
		instance.fuelAtEndOfJump = endValue;
		instance.fuelAtBeginningOfJump = startValue;
	}

	public static double CalculateDistance(RealSpacePostion to)
	{
		return new RealSpacePostion(to).Subtract(instance.pcsRSP).Magnitude();
	}

	private void UpdateEngineIntensityVisuallyAuto()
	{
		float percentage = normalEnginesIntensity / engineIntensityMax;

        UpdateEngineIntensityVisuallyDirect(percentage, percentage);
	}

	private void UpdateEngineIntensityVisuallyDirect(float shaderT, float uiT)
	{
		MainInfoUIControl.UpdateEngineBarInensity(uiT);
		Shader.SetGlobalFloat("_PCSEngineIntensity", shaderT);
	}

	private void Update()
	{
		if (!PlayerManagement.PlayerFactionExists())
		{
			return;
		}

		//Ship Movement
		if (jumpStage == JumpStage.Done)
		{
			float moveSpeedPercentage = Mathf.Max(0.0f, PlayerManagement.GetStats().GetStat(Stats.moveSpeed.ToString())) / 100.0f;

			//Not jumping
			bool inUINeutral = UIManagement.InNeutral();

			//Rotational Movement
			const float rotationalModifier = 0.01f;
			float rotationalChangeModifier = rotationalModifier * Time.deltaTime * moveSpeedPercentage;
			float modifierThisFrame = 0.0f;
			if (inUINeutral)
			{
                if (InputManagement.GetKey(InputManagement.rotateRightKey))
                {
                    modifierThisFrame = rotationalChangeModifier;
                }
                else if (InputManagement.GetKey(InputManagement.rotateLeftKey))
                {
                    modifierThisFrame = -rotationalChangeModifier;
                }
            }

			rotationalMovement += modifierThisFrame;


			//If no input or inverse input to current rotational movement
			if(modifierThisFrame == 0.0f ||
					(modifierThisFrame < 0.0f && rotationalMovement > 0.0f) ||
					(modifierThisFrame > 0.0f && rotationalMovement < 0.0f)
				)
			{
				rotationalMovement = Mathf.Lerp(rotationalMovement, 0.0f, rotationalChangeModifier * 200.0f);
			}

			rotationalMovement = MathHelper.ValueTanhFalloff(rotationalMovement, 4, -1);

			transform.rotation = Quaternion.Euler(
				transform.rotation.eulerAngles.x,
				transform.rotation.eulerAngles.y + rotationalMovement,
				transform.rotation.eulerAngles.z
				);

			//CameraManagement.AddRotationMainOnly(new Vector2(0, rotationalMovement));
			//

			//Engines
			const float engineAcceleration = 1.5f;
			float engineChangeModifier = engineAcceleration * moveSpeedPercentage * Time.deltaTime;

			if (inUINeutral)
			{
                if (InputManagement.GetKey(InputManagement.thrusterUpKey))
                {
                    normalEnginesIntensity += engineChangeModifier;
                }
                else if (InputManagement.GetKey(InputManagement.thrusterDownKey))
                {
                    normalEnginesIntensity = Mathf.MoveTowards(normalEnginesIntensity, 0.0f, engineChangeModifier);
                }
            }

			normalEnginesIntensity = Mathf.Clamp(normalEnginesIntensity, 0.0f, engineIntensityMax);
			UpdateEngineIntensityVisuallyAuto();

			Vector3 velocity = transform.forward * normalEnginesIntensity * moveSpeedPercentage;

			rigidbodyTarget.velocity = velocity;
			//
		}

		//Each frame disaplce the world center by amount moved
		//Position should be reset to zero zero when we finish a jump
		Vector3 moveDifference = transform.position - lastRecordedPos;
		moveDifference *= WorldManagement.invertedInEngineWorldScaleMultiplier;
		if (moveDifference.magnitude > 0.0f)
		{
			//If difference is non-existent 
			lastRecordedPos = transform.position;
			WorldManagement.MoveWorldCenter(moveDifference);

			if(transform.position.magnitude > 500.0f)
			{
				ResetPosition();
			}
		}

		//

		//JUMP ANIMATION
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

					//Only add rotation to the main camera
					//Don't want any rotation in map
					//CameraManagement.AddRotationMainOnly(new Vector2(0, difference));

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
					UpdateEngineIntensityVisuallyDirect(percentage, percentage);
				}
				else
				{
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

					SimulationManagement.SimulationSpeed(10);
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

					if (fuelAtEndOfJump != -1)
					{
						//Fuel is changing during jump
						MainInfoUIControl.UpdateFuelLabel(Mathf.Lerp(fuelAtBeginningOfJump, fuelAtEndOfJump, jumpT));
					}

					trail.SetPosition(1, new Vector3(0, 0, Mathf.Lerp(0, -maxTrailLength, jumpT * 15.0f)));
					WorldManagement.SetWorldCenterPosition(RealSpacePostion.Lerp(jumpStart, jumpEnd, jumpCurve.Evaluate(jumpT)));

					if (jumpT >= 1.0f)
					{
						//We have arrived
						SimulationManagement.SimulationSpeed(1);
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
	
		//

		//Update global effects
		UpdatePCSPosition(WorldManagement.worldCenterPosition);
	}

	private void FixedUpdate()
	{
		if (jumping)
		{
			//Force a clamp on velocity
			rigidbodyTarget.velocity = Vector3.zero;
		}
	}

	private void EndJump()
	{
		jumping = false;
		nextJumpAllowedTime = Time.time + 1.0f;
		UpdateEngineIntensityVisuallyDirect(0.0f, 0.0f);
		
		ResetPosition();
	}

	private void ResetPosition()
	{
		//Zero position
		posBeforeReset = transform.position;

        transform.position = Vector3.zero;
		lastRecordedPos = transform.position;

		//Make sure the camera doesn't just lerp to the new positions so it looks seamless
		CameraManagement.SetCameraPositionExternal(transform.position + CameraManagement.GetCameraDisplacementFromTarget(), true);

		//Invoke event
		onPositionReset.Invoke();
	}
}
