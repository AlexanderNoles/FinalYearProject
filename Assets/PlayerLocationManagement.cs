using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerLocationManagement : MonoBehaviour
{
	private static bool locationChanged = false;
	private static VisitableLocation previousLocation;
	private static VisitableLocation location;

	private void Awake()
	{
		locationChanged = true;
		location = null;

		//Will try to load position here from save file

		if (location == null)
		{
			//No location was loaded
			location = new ArbitraryLocation().SetLocation(new RealSpacePostion(0, 0, 50000));
		}

		//Run update once to get allow the system to run setup before any other updates run without duplicate code
		Update();
	}

	public static void UpdateLocation(VisitableLocation newLocation)
	{
		previousLocation = location;
		location = newLocation;
		locationChanged = true;
	}

	private void Update()
	{
#if UNITY_EDITOR
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
				UpdateLocation(newTarget.location);
			}
		}
#endif

		if (locationChanged)
		{
			if (previousLocation != null)
			{
				previousLocation.Cleanup();
				previousLocation = null;
			}

			if (location != null)
			{
				location.InitDraw();
				UpdateWorldPosition();
			}

			locationChanged = false;
		}

		if (location != null)
		{
			location.Draw();
		}
	}

	private void UpdateWorldPosition()
	{
		//Transfer world to new position
		//And place ship and camera at entry position
		RealSpacePostion centralPos = location.GetPosition();
		Vector3 offset = location.GetEntryOffset();
		//We want the ship to arrive at the entry position not camera, so we need to account for offset from the ship
		Vector3 cameraDisplacement = CameraManagement.GetCameraDisplacementFromTarget();

		//New RS world center pos is equal to the RS center + offset + camera displacement
		WorldManagement.SetWorldCenterPosition(new RealSpacePostion(
			centralPos.x + offset.x + cameraDisplacement.x,
			centralPos.y + offset.y + cameraDisplacement.y,
			centralPos.z + offset.z + cameraDisplacement.z
			));

		//New ship pos is relative to world center so it is just offset
		PlayerCapitalShip.SetRealWorldPos(offset);

		//New player RS pos is just the central pos + offset but as a RS pos
		PlayerCapitalShip.UpdatePCSPosition(new RealSpacePostion(
			centralPos.x + offset.x,
			centralPos.y + offset.y,
			centralPos.z + offset.z
			));

		PlayerCapitalShip.ModelLookAt(Vector3.zero);

		//New camera world pos is equal to offset + camera displacement
		//This needs to happen after setting PCS position as it uses it to disable the lerp correctly
		CameraManagement.SetCameraPositionExternal(offset + cameraDisplacement, true);
	}
}
