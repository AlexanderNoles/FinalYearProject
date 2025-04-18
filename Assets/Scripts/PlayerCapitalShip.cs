using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCapitalShip : MonoBehaviour
{
	public static UnityEvent onPositionReset = new UnityEvent();
	public static UnityEvent onJumpStart = new UnityEvent();

	public static State currentState = State.Normal;
	public enum State
	{
		Normal,
		Debug
	}

	[HideInInspector]
	public new Transform transform;

	public static Vector3 GetPosition()
	{
		return instance.transform.position;
	}

	public AnimationCurve turnCurve;
	public AnimationCurve jumpCurve;

	private static PlayerCapitalShip instance;
	private RealSpacePosition pcsRSP;
	private static bool jumping;

	public static bool IsJumping()
	{
		return jumping;
	}

	public static bool InJumpTravelStage()
	{
		return jumping && CurrentStage() > JumpStage.JumpBuildup;
	}

	public static JumpStage CurrentStage()
	{
		return jumpStage;
	}

	private static JumpStage jumpStage;
	private static VisitableLocation jumpTarget;

	public static RealSpacePosition GetTargetPosition()
	{
		return jumpTarget.GetPosition();
	}

	public static bool IsTargetPosition(RealSpacePosition pos)
	{
		if (pos == null || jumpTarget == null)
		{
			return false;
		}

		return pos.Equals(GetTargetPosition());
	}

	[MonitorBreak.Bebug.ConsoleCMD("quickjump", "Instantly jump to target location")]
	public static void ActivateDebugJumpMovementCMD()
	{
		debugJumpMovement = !debugJumpMovement;
	}

	private static bool debugJumpMovement = false;
	private static RealSpacePosition jumpEnd;
	private static RealSpacePosition jumpStart;
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

	public const float jumpBaseLineSpeed = 2500.0f;

	[MonitorBreak.Bebug.ConsoleCMD("SHIPJUMPSPEED")]
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
	public Transform modelTransform;
	private const float modelOffsetAfterJump = 1000;
	public AnimationCurve postJumpModelSlideInCurve;
	public ParticleSystem jumpPs;
	public Transform arcaneRingsParent;
	private List<Transform> arcaneRings = new List<Transform>();
	public LineRenderer trail;
	private const float maxTrailLength = 100.0f;
	public GameObject fireEffect;
	public GameObject portal;
	public MeshRenderer pulseRenderer;
	public MeshRenderer largePulseRenderer;
	private Material pulseMat;
	private Material largePulseMat;
	private float pulseT;
	public Transform piercer;

	public AnimationCurve backingBuildupCurve;
	public Transform backingEngineBuildup;
	public GameObject engineLine;

	public GameObject outerEffect;
	private Material outerMaterial;

	public List<TrailRenderer> TRtrails;

	private void Awake()
	{
		instance = this;
		jumping = false;

		onPositionReset.RemoveAllListeners();
		onJumpStart.RemoveAllListeners();

		transform = base.transform;
		lastRecordedPos = Vector3.zero;

		portal.SetActive(false);
		fireEffect.SetActive(false);
		engineLine.SetActive(false);
		outerEffect.SetActive(false);

		pulseMat = pulseRenderer.material;
		pulseMat.SetFloat("_T", 0.0f);

		largePulseMat = largePulseRenderer.material;
		largePulseMat.SetFloat("_T", 0.0f);

		outerMaterial = outerEffect.GetComponent<MeshRenderer>().material;

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

	public static void UpdatePCSPosition(RealSpacePosition pos)
	{
		instance.pcsRSP = pos;
		Shader.SetGlobalVector("_PCSPosition", instance.pcsRSP.AsTruncatedVector3(20000));
	}

	public static RealSpacePosition GetPCSPosition()
	{
		if (instance == null || instance.pcsRSP == null)
		{
			return WorldManagement.worldCenterPosition;
		}

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
		if (debugJumpMovement)
		{
			RealSpacePosition newWCP = new RealSpacePosition(newJumpTarget.GetPosition());
			newWCP.Add(Vector3.forward * (newJumpTarget.GetEntryOffset() * WorldManagement.invertedInEngineWorldScaleMultiplier));
			WorldManagement.worldCenterPosition = newWCP;
			return;
		}

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

		//Clone both below positions so we can edit them however we want
		//Get start position
		jumpStart = new RealSpacePosition(WorldManagement.worldCenterPosition);

		//Get final position
		jumpEnd = new RealSpacePosition(jumpTarget.GetPosition());

		//Setup rotation
		//Get difference between positions jumpBaseLineSpeed
		RealSpacePosition difference = new RealSpacePosition(jumpEnd).Subtract(jumpStart);
		thisJumpSpeed = (float)(jumpBaseLineSpeed / difference.Magnitude());

		Debug.Log(thisJumpSpeed);

		Vector3 vectorDifference = difference.AsTruncatedVector3(20000);

		lookAtTargetRot = Quaternion.LookRotation(vectorDifference, Vector3.up);
		startTurnRot = instance.transform.rotation;

		//Turn off trail renderer
		foreach (TrailRenderer tr in instance.TRtrails)
		{
			tr.enabled = false;
		}

		//Add entry offset to jump target
		jumpEnd.Subtract(jumpTarget.GetEntryOffset() * vectorDifference.normalized * WorldManagement.invertedInEngineWorldScaleMultiplier);

		//Setup control variables
		jumpT = 0.0f;
		rotateT = 0.0f;
		jumpBuildupBuffer = jumpBuildupMax;

		onJumpStart.Invoke();
	}

	public static void HaveFuelChangeOverJump(float startValue, float endValue)
	{
		instance.fuelAtEndOfJump = endValue;
		instance.fuelAtBeginningOfJump = startValue;
	}

	public static double CalculateDistance(RealSpacePosition to)
	{
		if (instance == null || instance.pcsRSP == null)
		{
			return 0;
		}

		return new RealSpacePosition(to).Subtract(instance.pcsRSP).Magnitude();
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

		foreach (TrailRenderer tr in TRtrails)
		{
			tr.time = shaderT;
		}
	}

	private void Update()
	{
		if (currentState == State.Debug)
		{
			//Get offset for this frame
			Vector3 orthagonalOffset = InputManagement.WASDInput();
			//Flip components
			(orthagonalOffset.x, orthagonalOffset.z) = (orthagonalOffset.z, orthagonalOffset.x);
			//Make relative to camera
			Vector3 moveVector = CameraManagement.MakeVectorRelativeToCameraDirection(orthagonalOffset).normalized;

			float speed = WorldManagement.invertedInEngineWorldScaleMultiplier;

			if (InputManagement.GetKey(KeyCode.LeftShift))
			{
				speed = 1;
			}
			else if (InputManagement.GetKey(KeyCode.LeftControl))
			{
				speed = 10;
			}

			WorldManagement.MoveWorldCenter(moveVector * speed);

			return;
		}

		if (!PlayerManagement.PlayerEntityExists())
		{
			return;
		}

		//Ship Movement
		if (jumpStage == JumpStage.Done)
		{
			float moveSpeedPercentage = Mathf.Max(0.0f, PlayerManagement.GetStats().GetStat(Stats.moveSpeed.ToString())) / 100.0f;

			//Not jumping
			bool inUINeutral = UIManagement.InPureNeutral();

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
				transform.rotation.eulerAngles.y + (rotationalMovement * 360.0f * Time.deltaTime),
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
			//If difference is existent 
			lastRecordedPos = transform.position;
			WorldManagement.MoveWorldCenter(moveDifference);

			if(transform.position.magnitude > 2000.0f)
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
					transform.rotation = Quaternion.Lerp(startTurnRot, lookAtTargetRot, turnCurve.Evaluate(rotateT));

					float effectIntensity = rotateT * 1.25f;
					UpdateEngineIntensityVisuallyDirect(effectIntensity, effectIntensity);

					if (rotateT >= 1.0f)
					{
						portal.SetActive(true);

						//Tell main info ui control we have started the jump proper
						MainInfoUIControl.SetEngineArcaneFlameActive(true);

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

					float piercerLength = 50000.0f;
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
					outerMaterial.SetFloat("_NearClamp", 15.0f);
					outerMaterial.SetFloat("_FarClamp", 15.0f);

					piercer.localScale = Vector3.zero;

					pulseT = 1.0f;

					//Setup trail
					trail.gameObject.SetActive(true);
					trail.SetPositions(new Vector3[2]
					{
						Vector3.zero,
						Vector3.zero
					});

					//Clear all attack targets
					PlayerSimObjBehaviour.ClearAllTargetsExternal();

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
					WorldManagement.SetWorldCenterPosition(RealSpacePosition.Lerp(jumpStart, jumpEnd, jumpCurve.Evaluate(jumpT)));

					if (jumpT >= 1.0f)
					{
						//We have arrived
						SimulationManagement.SimulationSpeed(1);
						jumpStage++;

						postJumpT = 0.0f;

						jumpPs.Stop();

						fireEffect.SetActive(false);

						backingEngineBuildup.localScale = Vector3.zero;
						engineLine.SetActive(false);
						outerEffect.SetActive(false);

						//currently just ending jump here
						EndJump();
						modelTransform.localPosition = Vector3.back * modelOffsetAfterJump;
					}

					const float effectModifer = 5f;
					outerMaterial.SetFloat("_NearClamp", Mathf.Lerp(15, -15, Mathf.Clamp01(jumpT * effectModifer)));
					float farT = Mathf.Clamp01((jumpT - (1.0f - (1.0f / effectModifer))) * effectModifer);
					outerMaterial.SetFloat("_FarClamp", Mathf.Lerp(15, -15, farT));

					for (int i = 0; i < arcaneRings.Count; i++)
					{
						Vector3 targetPosition = Mathf.Min(Mathf.Pow(jumpT, 2.0f) * 500.0f, 10.0f) * Vector3.back;

						arcaneRings[i].localPosition = Vector3.Lerp(arcaneRings[i].localPosition, targetPosition, 10.0f * Time.deltaTime);

						if (jumpT > 0.9f)
						{
							float clampedT = Mathf.Clamp01((jumpT - 0.9f) / 0.1f);
							arcaneRings[i].localScale = Vector3.Lerp(arcaneRings[i].localScale, Vector3.zero, clampedT);
						}
					}
				}
			}
		}

		if (jumpStage == JumpStage.PostJump)
		{
			postJumpT += Time.deltaTime;
			modelTransform.localPosition = Vector3.back * Mathf.Lerp(modelOffsetAfterJump, 0, postJumpModelSlideInCurve.Evaluate(postJumpT));

			largePulseMat.SetFloat("_T", postJumpT);

			trail.SetPosition(0, new Vector3(0, 0, Mathf.Lerp(0, -maxTrailLength, postJumpT)));

			GeneratorManagement.SetOffset(transform.forward * 500 * (1.0f - Mathf.Clamp01(postJumpT * 5f)));

			if (postJumpT >= 1.0f)
			{
				jumpStage++;
				trail.gameObject.SetActive(false);
				pulseT = 1.0f;
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
        MainInfoUIControl.SetEngineArcaneFlameActive(false);
        UpdateEngineIntensityVisuallyDirect(0.0f, 0.0f);

		ResetPosition();
	}

	private void ResetPosition()
	{
		//Zero position
		posBeforeReset = transform.position;
		transform.position = Vector3.zero;

		foreach (TrailRenderer tr in TRtrails)
		{
			tr.enabled = true;
			tr.Clear();
		}

		lastRecordedPos = transform.position;

		//Make sure the camera doesn't just lerp to the new positions so it looks seamless
		CameraManagement.SetCameraPositionExternal(transform.position + CameraManagement.GetCameraDisplacementFromTarget(), true);

		//Invoke event
		onPositionReset.Invoke();
	}

	[MonitorBreak.Bebug.ConsoleCMD("DebugMove")]
	public static void ActivateDebugMovement()
	{
		currentState = State.Debug;
		//Hide model
		instance.modelTransform.gameObject.SetActive(false);
	}

	[MonitorBreak.Bebug.ConsoleCMD("NormalMove")]
	public static void ActivateNormalMovement()
	{
		currentState = State.Normal;
		//Show the model
		instance.modelTransform.gameObject.SetActive(true);
	}
}
