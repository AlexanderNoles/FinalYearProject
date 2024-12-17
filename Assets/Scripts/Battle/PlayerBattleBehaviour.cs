using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleBehaviour : BattleBehaviour
{
	public List<Vector3> firePoints = new List<Vector3>();

	private const float maxSelectDistance = 500;

	protected override void Awake()
	{
		base.Awake();

		//DEBUG
		//Add test weapon
		weapons.Add(new WeaponProfile());
	}

	protected override Vector3 GetFireFromPosition(Vector3 targetPos)
	{
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
			float angle = Mathf.Abs(Vector3.Angle(lookDirection, displacement));

			if (angle < currentMinimum)
			{
				currentMinimum = angle;
				firePos = calculatedOrigin;
			}
		}

		return firePos;
	}

	private void Update()
	{
		//If player is not hovering over UI (or in the map) try to find any targets that are under mouse
		if (UIHelper.ElementsUnderMouse().Count <= 0 && !UIManagement.MapActive())
		{
			//Get mouse view ray
			Ray mouseViewRay = CameraManagement.GetMainCamera().ScreenPointToRay(Input.mousePosition);

			RaycastHit[] hits = Physics.RaycastAll(mouseViewRay, maxSelectDistance);

			//Find the closest object to the ray origin that is also a battle behaviour
			BattleBehaviour newTarget = null;
			float currentLowestRange = float.MaxValue;

			foreach (RaycastHit hit in hits)
			{
				if (BattleManagement.TryGetBattleBehaviour(hit.collider, out BattleBehaviour target))
				{
					if (hit.distance < currentLowestRange)
					{
						currentLowestRange = hit.distance;
						newTarget = target;
					}
				}
			}

			if (newTarget != null)
			{
				if (InputManagement.GetMouseButtonDown(InputManagement.MouseButton.Left))
				{
					//Yippee!
					ToggleTarget(newTarget);
				}
			}
		}

		//Process all current targets
		ProcessTargets();
	}

	//[ContextMenu("Calculate Dominant Components")]
	//public void CalculateDominantComponent()
	//{
	//	firePointDominantComponents.Clear();

	//	foreach (Vector3 firepoint in firePoints)
	//	{
	//		Vector3 calculatedDominantComponent;
	//		if (Mathf.Abs(firepoint.x) > Mathf.Abs(firepoint.z))
	//		{
	//			//X component is dominant
	//			calculatedDominantComponent = Vector3.right * (firepoint.x / Mathf.Abs(firepoint.x));
	//		}
	//		else
	//		{
	//			//Z component is dominamt
	//			calculatedDominantComponent = Vector3.forward * (firepoint.z / Mathf.Abs(firepoint.z));
	//		}

	//		firePointDominantComponents.Add(calculatedDominantComponent);
	//	}
	//}
}
