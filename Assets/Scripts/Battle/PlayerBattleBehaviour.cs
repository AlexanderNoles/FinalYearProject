using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattleBehaviour : BattleBehaviour
{
	private const float maxSelectDistance = 500;

	protected override void Awake()
	{
		base.Awake();

		//DEBUG
		//Add test weapon
		weapons.Add(new WeaponProfile());
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
}
