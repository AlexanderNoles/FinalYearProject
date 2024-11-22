using System.Collections;
using System.Collections.Generic;
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
			Vector3 pos = Random.onUnitSphere;
			pos.y = 0;
			double range = WorldManagement.GetSolarSystemRadius() * 0.15f;

			UpdateLocation(new ArbitraryLocation().SetLocation(
				new RealSpacePostion(pos.x * range, 0, pos.z * range)
				));
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
		//This places the entry position equivalent to 0, 0 inengine
		RealSpacePostion pos = location.GetEntryPosition();
		Vector3 displacement = transform.position;

		WorldManagement.SetWorldCenterPosition(new RealSpacePostion(
			pos.x + displacement.x,
			pos.y + displacement.y,
			pos.z + displacement.z
			));

		//Update the player capital ship position (in simulation)
		PlayerCapitalShip.UpdatePCSPosition(new RealSpacePostion(pos.x, pos.y, pos.z));
	}
}
