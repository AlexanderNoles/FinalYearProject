using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PlayerLocationManagement : MonoBehaviour
{
	private static bool locationChanged = false;
	private static List<VisitableLocation> previousLocation;
	private static List<VisitableLocation> location;
	private static List<VisitableLocation> preparedWarpLocation = null;

	public static UnityEvent onLocationChanged = new UnityEvent();

	public static VisitableLocation GetCurrentPrimaryLocation()
	{
		if (PlayerCapitalShip.IsJumping())
		{
			return preparedWarpLocation[0];
		}

		if (location.Count == 0)
		{
			return null;
		}

		return location[0];
	}

	public static List<VisitableLocation> GetCurrentLocation()
	{
		if (location == null)
		{
			if (PlayerCapitalShip.IsJumping())
			{
				return preparedWarpLocation;
			}
		}

		return GetCurrentLocationRaw();
	}

	public static List<VisitableLocation> GetCurrentLocationRaw()
	{
		return location;
	}

	public static bool IsPlayerLocation(RealSpacePostion pos)
	{
		foreach (VisitableLocation location in location)
		{
			if (location.GetPosition().Equals(pos))
			{
				return true;
			}
		}

		return false;
	}

	public static bool IsPlayerLocation(List<VisitableLocation> locations)
	{
		foreach (VisitableLocation location in locations)
		{
			if (IsPlayerLocation(location)) 
			{
				return true;
			}
		}

		return false;
	}

	public static bool IsPlayerLocation(VisitableLocation location)
	{
		if (PlayerLocationManagement.location.Count == 0)
		{
			return false;
		}

		return PlayerLocationManagement.location.Contains(location);
	}

	private void Awake()
	{
		location = new List<VisitableLocation>();
		previousLocation = new List<VisitableLocation>();           
		//This acts as a location to display when the ship is traveling throught the warp
		preparedWarpLocation = new List<VisitableLocation>() { new WarpLocation() };

		//Remove any leftover listeners
		onLocationChanged.RemoveAllListeners();

		locationChanged = true;

		//Will try to load position here from save file

		if (location.Count == 0)
		{
			//No location was loaded
			AddLocation(new ArbitraryLocation().SetLocation(new RealSpacePostion(0, 0, 20000)));
		}

		//Run update once to get allow the system to run setup before any other updates run without duplicate code
		LateUpdate();
	}

	private static void AddLocation(VisitableLocation newLocation)
	{
		location.Add(newLocation);
	}

	public static void UpdateLocation(List<VisitableLocation> newLocation)
	{
		previousLocation = location;
		ChangeLocation(newLocation);
		locationChanged = true;
	}

	public static void ForceUnloadCurrentLocation()
	{
		if (location != null)
		{
			foreach (VisitableLocation location in location)
			{
				location.Cleanup();
			}

			ChangeLocation(new List<VisitableLocation>());
		}
	}

	//Automatically triggers the OnLocationChangeEvent
	private static void ChangeLocation(List<VisitableLocation> newLocation)
	{
		location = newLocation;
		onLocationChanged.Invoke();

	}

	private void LateUpdate()
	{
		if (locationChanged)
		{
			if (previousLocation.Count != 0)
			{
				foreach(VisitableLocation location in previousLocation)
				{
					location.Cleanup();
				}

				previousLocation.Clear();
			}

			if (location != null)
			{
				foreach (VisitableLocation location in location)
				{
					location.InitDraw();
				}

				UpdateWorldPosition();
			}

			locationChanged = false;
		}

		if (location.Count != 0)
		{
			foreach (VisitableLocation location in location)
			{
				location.Draw();
			}
		}
	}

	private void UpdateWorldPosition()
	{
		RealSpacePostion centralPos = GetCurrentPrimaryLocation().GetPosition();
		WorldManagement.SetWorldCenterPosition(centralPos);
		PlayerCapitalShip.UpdatePCSPosition(WorldManagement.worldCenterPosition);
	}
}
