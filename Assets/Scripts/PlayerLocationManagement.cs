using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PlayerLocationManagement : MonoBehaviour
{
	private static bool locationChanged = false;
	private static VisitableLocation previousLocation;
	private static VisitableLocation location;

	public static UnityEvent onLocationChanged = new UnityEvent();

	public static bool IsPlayerLocation(RealSpacePostion pos)
	{
		if (location == null)
		{
			return false;
		}

		return location.GetPosition().Equals(pos);
	}

	public static bool IsPlayerLocation(VisitableLocation location)
	{
		if (PlayerLocationManagement.location == null)
		{
			return false;
		}

		return PlayerLocationManagement.location.Equals(location);
	}

	private void Awake()
	{
		//Remove any leftover listeners
		onLocationChanged.RemoveAllListeners();

		locationChanged = true;
		location = null;

		//Will try to load position here from save file

		if (location == null)
		{
			//No location was loaded
			location = new ArbitraryLocation().SetLocation(new RealSpacePostion(0, 0, 50000));
		}

		//Run update once to get allow the system to run setup before any other updates run without duplicate code
		LateUpdate();
	}

	public static void UpdateLocation(VisitableLocation newLocation)
	{
		previousLocation = location;
		ChangeLocation(newLocation);
		locationChanged = true;
	}

	public static void ForceUnloadCurrentLocation()
	{
		if (location != null)
		{
			location.Cleanup();
			ChangeLocation(null);
		}
	}

	//Automatically triggers the OnLocationChangeEvent
	private static void ChangeLocation(VisitableLocation newLocation)
	{
		location = newLocation;
		onLocationChanged.Invoke();

	}

	private void LateUpdate()
	{
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
		RealSpacePostion centralPos = location.GetPosition();
		WorldManagement.SetWorldCenterPosition(centralPos);
		PlayerCapitalShip.UpdatePCSPosition(WorldManagement.worldCenterPosition);
	}
}
