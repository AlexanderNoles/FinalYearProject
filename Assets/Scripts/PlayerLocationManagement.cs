using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityAndDataDescriptor;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerLocationManagement : MonoBehaviour
{
	private static PlayerLocationManagement instance;
	public static UnityEvent onLocationChanged = new UnityEvent();

	public static bool IsPlayerLocation(VisitableLocation location)
	{
		if (instance != null)
		{
			foreach (DrawnLocation entry in instance.drawnLocations)
			{
				if (entry.targetLocation.Equals(location))
				{
					return true;
				}
			}
		}

		return false;
	}

	public static bool IsPlayerLocation(RealSpacePostion pos)
	{
		if (instance != null)
		{
			foreach (DrawnLocation entry in instance.drawnLocations)
			{
				if (entry.targetLocation.GetPosition().Equals(pos))
				{
					return true;
				}
			}
		}

		return false;
	}

	private const double normalMaxDistance = 150;

	public static DrawnLocation GetPrimaryLocationWrapper(double maxDistance = normalMaxDistance)
	{
		if (PlayerCapitalShip.IsJumping() && PlayerCapitalShip.CurrentStage() > PlayerCapitalShip.JumpStage.JumpBuildup)
		{
			//Player ship is inside warp
			return warpLocation;
		}

		//No nearby locations or no player location instance
		if (instance == null || instance.drawnLocations.Count == 0 || instance.correspondingDistances[0] > maxDistance * WorldManagement.invertedInEngineWorldScaleMultiplier)
		{
			return backupLocation;
		}

		//
		return instance.drawnLocations[0];
	}

	public static VisitableLocation GetPrimaryLocation(double maxDistance = normalMaxDistance)
	{
		return GetPrimaryLocationWrapper(maxDistance).targetLocation;
	}

	private static DrawnLocation warpLocation = null;
	private static DrawnLocation backupLocation = null;
	private List<DrawnLocation> drawnLocations = new List<DrawnLocation>();
	private List<DrawnLocation> newDrawnLocations = new List<DrawnLocation>();
	private List<double> correspondingDistances = new List<double>();
	private Func<VisitableLocation, int> locationGetOperation;

	private int sessionNextID;
	private int currentPrimaryLocationID;

	public class DrawnLocation
	{
		public int locationID = -1;

		public VisitableLocation targetLocation;
		public Transform parent;

        public override bool Equals(object obj)
        {
			if (obj == null || obj.GetType() != typeof(DrawnLocation))
			{
				return false;
			}

            return targetLocation.Equals(((DrawnLocation)obj).targetLocation);
        }

        public override int GetHashCode()
        {
			if (targetLocation == null)
			{
				return 0;
			}

            return targetLocation.GetHashCode();
        }

		public void SetID(int newID)
		{
			locationID = newID;
		}

		public void InitParent()
		{
			//Create new parent object
			//Could use an object pool for this but I don't think it really matters with such a small amount of nearby locations
			parent = new GameObject("DrawnLocation").transform;
		}

		public void Cleanup()
		{
			Destroy(parent.gameObject);
		}

		public void SetPosAsOffsetFrom(RealSpacePostion rsp, Vector3 additionalOffset)
		{
			//Calculate difference between rsp and target location rsp
			RealSpacePostion difference = targetLocation.GetPosition().SubtractToClone(rsp);

			//Set that as our offset plus an additional offset
			parent.position = (difference.AsVector3() * WorldManagement.inEngineWorldScaleMultiplier) + additionalOffset;
		}

		public Vector3 GetWorldPosition()
		{
			return parent.position;
		}
    }

	public MultiObjectPool uiPool;
	public RectTransform uiPoolTargetRect;
	private const int drawnLocationTargetIndex = 0;
	private Dictionary<RectTransform, LocationTrackingUI> transformToLocationTracker = new Dictionary<RectTransform, LocationTrackingUI>();

	private void Awake()
	{
		if (warpLocation == null)
		{
			warpLocation = new DrawnLocation();
			warpLocation.SetID(0);
			warpLocation.targetLocation = new WarpLocation();

			backupLocation = new DrawnLocation();
			warpLocation.SetID(1);
			backupLocation.targetLocation = new WorldCenterLocation();
		}

		//Set inital world center position
		WorldManagement.SetWorldCenterPosition(new RealSpacePostion(0, 0, 20000));

		instance = this;
		sessionNextID = 2;
		currentPrimaryLocationID = -1;

		//Define operation that is used to get surrounding locations for drawing
		//Scan all surrounding cells, if a location is within draw range:
		//	IF we are currently drawing it, then remove it from DrawnLocation list and add it to the updated list
		//	IF we are not currently drawing it, then add it to an init list to be drawn as well as the new updated list
		//	Any locations remaining in the old list can then be cleaned up (un drawn)
		locationGetOperation = delegate(VisitableLocation visitableLocation) 
		{
			//Are we currently drawing this location
			DrawnLocation newDrawnLocation = new DrawnLocation();
			bool currentlyDrawing = false;

			for (int i = 0; i < drawnLocations.Count; i++)
			{
				if (drawnLocations[i].targetLocation.Equals(visitableLocation))
				{
					newDrawnLocation = drawnLocations[i];

					drawnLocations.RemoveAt(i);
					currentlyDrawing = true;
					break;
				}
			}

			if (!currentlyDrawing)
			{
				newDrawnLocation.InitParent();

				newDrawnLocation.SetID(sessionNextID);
				sessionNextID++;

				visitableLocation.InitDraw(newDrawnLocation.parent);
				newDrawnLocation.targetLocation = visitableLocation;
			}

			//Add location to new list based on it's distance
			//This means the closest location will always be the start of the list
			double distance = visitableLocation.GetPosition().Distance(WorldManagement.worldCenterPosition);

			int index;
			for (index = 0; index < newDrawnLocations.Count;)
			{
				if (distance < correspondingDistances[index])
				{
					break;
				}
				else
				{
					index++;
				}	
			}

			newDrawnLocations.Insert(index, newDrawnLocation);
			correspondingDistances.Insert(index, distance);
			return 0;
		};
	}
	
	private void LateUpdate()
	{
		if (!PlayerManagement.PlayerEntityExists())
		{
			return;
		}

		RealSpacePostion worldCenter = WorldManagement.worldCenterPosition;

		//Reset list as new object
		newDrawnLocations = new List<DrawnLocation>();
		correspondingDistances.Clear();

		//Perform operation
		double drawDistance = 100 * WorldManagement.inEngineWorldScaleMultiplier;
		int chunkRange = (int)Math.Ceiling(drawDistance / WorldManagement.GetGridDensity());
		PerformOperationOnNearbyLocations(worldCenter, locationGetOperation, chunkRange, 1, drawDistance);

		//Remove any remaining locations in drawn locations
		foreach (DrawnLocation location in drawnLocations)
		{
			location.targetLocation.Cleanup();
			location.Cleanup();
		}

		//Replace drawn locations
		drawnLocations = newDrawnLocations;
		//Update position of all drawn locations based on offset from world center
		//and the player ships world (in engine) position
		//Then run the draw update method
		Camera mainCamera = CameraManagement.GetMainCamera();
		Vector2 targetRectSizeDelta = uiPoolTargetRect.sizeDelta;
		Vector2 halfTRSD = targetRectSizeDelta * 0.5f;

        foreach (DrawnLocation location in drawnLocations)
		{
			location.SetPosAsOffsetFrom(worldCenter, PlayerCapitalShip.GetPosition());
			location.targetLocation.DrawUpdate();

			//Set position of ui indicator
			Vector3 worldPos = location.GetWorldPosition();
			Vector3 viewPortPos = mainCamera.WorldToViewportPoint(worldPos);

			if (viewPortPos.z > 0.0f)
            {
                RectTransform uiIndicator = uiPool.UpdateNextObjectPosition(drawnLocationTargetIndex, Vector3.zero).transform as RectTransform;

                uiIndicator.anchoredPosition3D = new Vector2(viewPortPos.x * targetRectSizeDelta.x, viewPortPos.y * targetRectSizeDelta.y) - halfTRSD;
				//uiIndicator.localScale = Mathf.Clamp((100.0f / Vector3.Distance(worldPos, CameraManagement.GetMainCameraPosition())), 0.25f, 1.0f) * 3.0f * Vector3.one;

				if (!transformToLocationTracker.ContainsKey(uiIndicator))
				{
                    transformToLocationTracker.Add(uiIndicator, uiIndicator.GetComponent<LocationTrackingUI>());
				}

				LocationTrackingUI targetUI = transformToLocationTracker[uiIndicator];
				string locationTitle = location.targetLocation.GetTitle();
				if (!targetUI.label.text.Equals(locationTitle))
				{
                    targetUI.label.text = locationTitle;
				}

				//Calculate distance from worldCenter
				double distance = WorldManagement.OffsetFromWorldCenter(location.targetLocation.GetPosition(), Vector3.zero).Magnitude();
				string distanceText = Math.Round(distance).ToString() + " u";
				if (!targetUI.distanceLabel.text.Equals(distanceText))
				{
					targetUI.distanceLabel.text = distanceText;
                }
            }
        }

		uiPool.PruneObjectsNotUpdatedThisFrame(drawnLocationTargetIndex, true);

        //Set primary location id
        int newPrimaryLocationID = GetPrimaryLocationWrapper().locationID;

		//If primary location has changed, run onLocationChanged event
		if (currentPrimaryLocationID != newPrimaryLocationID)
		{
			currentPrimaryLocationID = newPrimaryLocationID;
			onLocationChanged.Invoke();
		}
	}

	public static void PerformOperationOnNearbyLocations(RealSpacePostion pos, Func<VisitableLocation, int> operation, int chunkRange = 1, int buffer = 1, double distanceClamp = -1)
	{
		//Grab data needed to compute
		List<SettlementData> setData = SimulationManagement.GetDataViaTag(DataTags.Settlement).Cast<SettlementData>().ToList();
		List<CapitalData> capitalData = SimulationManagement.GetDataViaTag(DataTags.Capital).Cast<CapitalData>().ToList();
		GameWorld.main.GetData(DataTags.GlobalBattle, out GlobalBattleData globalBattle);

		//Get postion to run check from
		RealSpacePostion currentPlayerCellCenter = WorldManagement.ClampPositionToGrid(pos);
		
		double density = WorldManagement.GetGridDensity();

		//Get the players offset from their cell center
		//The range check works on a cell basis but distance should be measured from player's actual position
		//So the x and z need to be offset so we can calculate correctly
		float playerXOffsetFromCellCenter = (float)((currentPlayerCellCenter.x - pos.x) / density);
		float playerZOffsetFromCellCenter = (float)((currentPlayerCellCenter.z - pos.z) / density);

		int bufferedChunkRange = chunkRange + buffer;

		List<VisitableLocation> foundLocations = new List<VisitableLocation>();

		for (int x = -bufferedChunkRange; x <= bufferedChunkRange; x++)
		{
			for (int z = -bufferedChunkRange; z <= bufferedChunkRange; z++)
			{
				//Within circle
				if (Mathf.Abs(x + playerXOffsetFromCellCenter) + Mathf.Abs(z + playerZOffsetFromCellCenter) <= bufferedChunkRange)
				{
					//Find offset from cell center
					RealSpacePostion currentCellCenter = new RealSpacePostion(
						currentPlayerCellCenter.x + (density * x), 
						0, 
						currentPlayerCellCenter.z + (density * z));

					foundLocations.Clear();

					//Iterate through all captured data
					//If they have a location (that we have decided is relevant to the player) in this chunk place it on the map
					foreach (SettlementData set in setData)
					{
                        if (set.settlements.ContainsKey(currentCellCenter))
                        {
                            foundLocations.Add(set.settlements[currentCellCenter].location);
                        }
                    }

					foreach (CapitalData cap in capitalData)
					{
                        if (cap.position != null)
                        {
                            if (cap.position.Equals(currentCellCenter))
                            {
                                foundLocations.Add(cap.location);
                            }
                        }
                    }

                    if (globalBattle.battles.ContainsKey(currentCellCenter))
                    {
                        foundLocations.Add(globalBattle.battles[currentCellCenter]);
                    }
					//

					foreach (VisitableLocation location in foundLocations)
					{
						//No distance check or distance check passed
						if (distanceClamp == -1 || location.GetPosition().SubtractToClone(pos).Magnitude() <= distanceClamp)
						{
							operation.Invoke(location);
						}
					}
				}
			}
		}
	}
}
