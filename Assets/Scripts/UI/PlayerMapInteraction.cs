using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMapInteraction : PostTickUpdate
{
	public MultiObjectPool mapPools;

	[Header("Effects")]
	public Transform targetIcon;
	public Transform rangeIndicator;
	private Material rangeIndicatorMat;
	private float cachedRange;
	public Transform playerDirectionIndicator;

	[Header("Location Information Disaply")]
	public Canvas canvas;
	private RectTransform canvasRect;
	public RectTransform floatingLocationInformationPopup;
	public TextMeshProUGUI locationTitleLabel;
	public TextMeshProUGUI locationDescLabel;
	public TextMeshProUGUI fuelCostLabel;
	public TextMeshProUGUI compoundLabel;

	private class LocationOnMap
	{
		public Vector3 worldPos;
		public VisitableLocation actualLocationData;
	}

	private class LOMCompound
	{
		public LocationOnMap primaryLocation;
		public List<LocationOnMap> secondaryLocations = new List<LocationOnMap>();

		public List<VisitableLocation> GetLocationsPackaged()
		{
			List<VisitableLocation> locations = new List<VisitableLocation>() { primaryLocation.actualLocationData };

			foreach (LocationOnMap location in secondaryLocations)
			{
				locations.Add(location.actualLocationData);
			}

			return locations;
		}
	}

	private bool doneInitialDraw;
	private Dictionary<Vector3, List<LOMCompound>> cellCenterToLocations = new Dictionary<Vector3, List<LOMCompound>>();
	private Dictionary<Transform, SpriteRenderer> transformToLocationRenderer = new Dictionary<Transform, SpriteRenderer>();

	private Func<VisitableLocation, int> getPositionsOperation;

	private void Awake()
	{
		//Create operations
		getPositionsOperation = delegate (VisitableLocation location) 
		{
			AddPosition(-location.GetPosition().AsTruncatedVector3(UIManagement.mapRelativeScaleModifier), location);

			return 0;
		};
		//

		canvasRect = canvas.transform as RectTransform;
		rangeIndicatorMat = rangeIndicator.GetComponent<MeshRenderer>().material;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		rangeIndicator.gameObject.SetActive(false);
		targetIcon.gameObject.SetActive(false);
		Shader.SetGlobalFloat("_ShipRange", 0.0f);
		doneInitialDraw = false;
		mapPools.HideAllObjects(4);
	}

	protected override void PostTick()
	{
		if (!doneInitialDraw)
		{
			return;
		}

		DrawAvaliableLocations();
	}

	public void DrawAvaliableLocations()
	{
		mapPools.PruneObjectsNotUpdatedThisFrame(4);
		//Need to first collect all avaliable locations
		//Then draw those locations on the map
		//Store information about the found locations so we can see if the player is trying to go there and then send them there

		//Reset locations
		cellCenterToLocations.Clear();

		if (PlayerCapitalShip.IsJumping())
		{
			//Do nothing
			//The Warp doesn't implement get position anyway so the below code would throw an error
			return;
		}

		int chunkRange = 10;
		List<Faction> players = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);

		if (players.Count > 0)
		{
			players[0].GetData(PlayerFaction.statDataKey, out PlayerStats playerStats);
			chunkRange = Mathf.FloorToInt(playerStats.GetStat(Stats.jumpRange.ToString()));
		}

		const float buffer = 1;
		float calculatedRange = (chunkRange * density) + buffer;
		cachedRange = calculatedRange;

		rangeIndicatorMat.SetFloat("_Radius", calculatedRange);

		PlayerLocationManagement.PerformOperationOnNearbyLocations(WorldManagement.worldCenterPosition, getPositionsOperation, chunkRange);
	}

	private void AddPosition(Vector3 worldPos, VisitableLocation location)
	{
		if (PlayerLocationManagement.IsPlayerLocation(location))
		{
			//Can't travel to same location
			return;
		}

		Transform newLocationTrans = mapPools.UpdateNextObjectPosition(4, worldPos);

		if (!transformToLocationRenderer.ContainsKey(newLocationTrans))
		{
			transformToLocationRenderer.Add(newLocationTrans, newLocationTrans.GetComponent<SpriteRenderer>());
		}

		transformToLocationRenderer[newLocationTrans].color = location.GetMapColour();

		//Get on map cell center
		Vector3 cellCenter = ClampPositionToGridMap(worldPos);

		//Construct Location On Map
		LocationOnMap newLocation = new LocationOnMap();
		newLocation.actualLocationData = location;
		newLocation.worldPos = worldPos;

		//If this location is very close to another then we need to compound it with others
		LOMCompound mergeTarget = null;
		float minimumDistance = float.MaxValue;

		PerformOperationOnSurroundingCells(cellCenter, new Func<Vector3, int>((Vector3 cellCenter) =>
		{
			foreach (LOMCompound compound in cellCenterToLocations[cellCenter])
			{
				//Find distance from primary compound entry
				float distance = (compound.primaryLocation.worldPos - worldPos).magnitude;

				//if close
				const float closeThreshold = 0.5f;
				if (distance < closeThreshold)
				{
					if (distance < minimumDistance)
					{
						mergeTarget = compound;
						minimumDistance = distance;
					}
				}
			}

			return 0;
		}));

		if (mergeTarget != null)
		{
			//If any merge target was found we merge into that
			mergeTarget.secondaryLocations.Add(newLocation);
		}
		else
		{
			//Otherwise we need to simply create a new compound 
			if (!cellCenterToLocations.ContainsKey(cellCenter))
			{
				cellCenterToLocations.Add(cellCenter, new List<LOMCompound>());
			}

			LOMCompound newCompound = new LOMCompound();
			newCompound.primaryLocation = newLocation;
			cellCenterToLocations[cellCenter].Add(newCompound);
		}
	}

	const float density = 3;

	private static Vector3 ClampPositionToGridMap(Vector3 worldPos)
	{
		return new Vector3(Mathf.Round(worldPos.x / density), 0, Mathf.Round(worldPos.z / density)) * density;
	}

	private Plane xzPlane = new Plane(Vector3.up, Vector3.zero);

	protected override void Update()
	{
		base.Update();

		//Each frame find the cell closest to the player's mouse position projected from the camera onto the x-z plane
		//Perform a 3 by 3 check on all the cells near that and find the closest position to the players mouse

		//If they click then start jump to that position

		Vector3 pos = -WorldManagement.worldCenterPosition.AsTruncatedVector3(UIManagement.mapRelativeScaleModifier);
		rangeIndicator.position = pos + Vector3.up * 0.05f;
		Shader.SetGlobalVector("_WCMapPos", pos);

		playerDirectionIndicator.position = pos;
		playerDirectionIndicator.LookAt(pos - PlayerCapitalShip.GetForward());

		if (UIManagement.MapIntroRunning())
		{
			if (UIManagement.LastFrameMapIntroRunning())
			{
				rangeIndicator.gameObject.SetActive(true);
			}

			return;
		}
		else
		{
			if (!doneInitialDraw)
			{
				doneInitialDraw = true;
				DrawAvaliableLocations();
			}
			Shader.SetGlobalFloat("_ShipRange", cachedRange);
		}

		if (PlayerCapitalShip.IsJumping() || UIHelper.ElementsUnderMouse().Count > 0)
		{
			//Don't let them set a new position if we are jumping or if we are currently  moused over a ui object
			targetIcon.gameObject.SetActive(false);
			return;
		}

		//Intersection with x z plane
		Ray mouseViewRay = CameraManagement.GetBackingCamera().ScreenPointToRay(Input.mousePosition);

		//If we intersect with the plane
		if (xzPlane.Raycast(mouseViewRay, out float enter))
		{
			Vector3 hitPoint = mouseViewRay.GetPoint(enter);

			Debug.DrawRay(hitPoint, Vector3.up * 100.0f, Color.blue);

			LOMCompound targetLocation = null;
			float currentShortestDistance = float.MaxValue;

			const float mouseDistanceBuffer = 2;

			PerformOperationOnSurroundingCells(hitPoint, new Func<Vector3, int>((Vector3 cellCenter) => 
			{
				//Any locations in this cell
				foreach (LOMCompound locationOnMap in cellCenterToLocations[cellCenter])
				{
					float sqrDistance = (locationOnMap.primaryLocation.worldPos - hitPoint).sqrMagnitude;

					if (sqrDistance <= mouseDistanceBuffer && sqrDistance < currentShortestDistance)
					{
						currentShortestDistance = sqrDistance;
						targetLocation = locationOnMap;
					}
				}

				return 0;
			}));

			bool foundLocation = targetLocation != null;
			targetIcon.gameObject.SetActive(foundLocation);

			if (foundLocation)
			{
				//Disaply target information
				//Currently just displaying the primary locations information
				targetIcon.position = targetLocation.primaryLocation.worldPos;
				locationTitleLabel.text = targetLocation.primaryLocation.actualLocationData.GetTitle();
				locationDescLabel.text = targetLocation.primaryLocation.actualLocationData.GetDescription();

				//Move window to be below mouse
				RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, canvas.worldCamera, out Vector2 mousePos);
				floatingLocationInformationPopup.position = canvasRect.TransformPoint(mousePos);

				//Calculate fuel cost
				int fuelCost;

				//Fuel is calculated in this script because this form of travel uses fuel
				//Some forms of travel won't so it's not done directly in the jump function
				double distance = PlayerCapitalShip.CalculateDistance(targetLocation.primaryLocation.actualLocationData.GetPosition());
				fuelCost = (int)Math.Floor(distance / 100);

				//Disaply fuel cost
				fuelCostLabel.text = fuelCost.ToString();

				//Display number of locations compounded into this one
				string compoundLabelText = string.Empty;

				if (targetLocation.secondaryLocations.Count > 0)
				{
					compoundLabelText = $"+{targetLocation.secondaryLocations.Count}";
				}

				compoundLabel.text = compoundLabelText;

				//Get inventory
				List<Faction> players = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);
				if (players.Count > 0)
				{
					players[0].GetData(PlayerFaction.inventoryDataKey, out PlayerInventory inventory);

					if (inventory.fuel >= fuelCost)
					{
						fuelCostLabel.color = Color.green;

						if (InputManagement.GetMouseButtonDown(InputManagement.MouseButton.Left))
						{
							PlayerCapitalShip.StartJump(targetLocation.primaryLocation.actualLocationData);

							//Remove fuel and have player ship change the ui label over the course of the jump
							PlayerCapitalShip.HaveFuelChangeOverJump(inventory.fuel, inventory.fuel - fuelCost);
							inventory.fuel -= fuelCost;

							//Set initial draw back to false so when the player arrives the inital draw is done again
							//Otherwise locations won't show up till next post tick call
							doneInitialDraw = false;

							mapPools.HideAllObjects(4);
						}
					}
					else
					{
						fuelCostLabel.color = Color.red;
					}
				}
			}
		}
	}

	private void PerformOperationOnSurroundingCells(Vector3 pos, Func<Vector3, int> operation, int size = 1)
	{
		Vector3 cellCenter = ClampPositionToGridMap(pos);

		for (int x = -size; x <= size; x++)
		{
			for (int z = -size; z <= size; z++)
			{
				//Calculate cell center to check
				Vector3 currentCellCenter = cellCenter + (new Vector3(x, 0, z) * density);

				if (cellCenterToLocations.ContainsKey(currentCellCenter))
				{
					if (operation(currentCellCenter) != 0)
					{
						return;
					}
				}
			}
		}
	}
}
