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
	private float growthOnLocationT;

	private static float jumpBuildupBuffer;

	private const float jumpBaseLineSpeed = 10000.0f;
	private static float thisJumpSpeed;

	private enum JumpStage
	{
		InitialTurn,
		JumpBuildup,
		ActualJump,
		PostJump,
		Done
	}


	private void Awake()
	{
		instance = this;
		jumping = false;

		transform = base.transform;
	}

	public static void UpdatePCSPosition(RealSpacePostion pos)
	{
		instance.pcsRSP = pos;
		Shader.SetGlobalVector("_PCSPosition", instance.pcsRSP.TruncatedVector3(20000));
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

		jumping = true;
		jumpStage = JumpStage.InitialTurn;
		jumpTarget = target;

		//Clone the position
		jumpStart = new RealSpacePostion(WorldManagement.worldCenterPosition);

		//Setup rotation
		//Get difference between positions jumpBaseLineSpeed
		RealSpacePostion difference = new RealSpacePostion(target.GetPosition()).Subtract(instance.pcsRSP);
		thisJumpSpeed = (float)(jumpBaseLineSpeed / difference.Magnitude());

		Vector3 vectorDifference = difference.TruncatedVector3(20000);

		lookAtTargetRot = Quaternion.LookRotation(vectorDifference, Vector3.up);
		startTurnRot = instance.transform.rotation;

		//Setup control variables
		jumpT = 0.0f;
		rotateT = 0.0f;
		jumpBuildupBuffer = 3.0f;
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
					transform.rotation = Quaternion.Lerp(startTurnRot, lookAtTargetRot, turnCurve.Evaluate(rotateT));

					if (rotateT >= 1.0f)
					{
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
				}
				else
				{
					PlayerLocationManagement.ForceUnloadCurrentLocation();
					jumpStage++;
				}
			}
			else if (jumpStage == JumpStage.ActualJump)
			{
				//Lerp the world center from current to target
				if (jumpT < 1.0f)
				{
					jumpT += Time.deltaTime * thisJumpSpeed;

					WorldManagement.SetWorldCenterPosition(RealSpacePostion.Lerp(jumpStart, jumpTarget.GetPosition(), jumpCurve.Evaluate(jumpT)));
					UpdatePCSPosition(WorldManagement.worldCenterPosition);

					if (jumpT >= 1.0f)
					{
						//We have arrived
						jumpStage++;

						growthOnLocationT = 0.0f;

						//currently just ending jump here
						EndJump();
					}
				}
			}
		}

		if (jumpStage == JumpStage.PostJump)
		{
			growthOnLocationT += Time.deltaTime * 5.0f;

			GeneratorManagement.SetOffset(-transform.forward * 50 * Mathf.Clamp01(growthOnLocationT));

			if (growthOnLocationT >= 1.0f)
			{
				jumpStage++;
			}
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
