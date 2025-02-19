using EntityAndDataDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static PlayerMapInteraction;

public class PlayerMapInteraction : PostTickUpdate
{
	public static PlayerMapInteraction instance;

	//Used primarily for hover checks for ui elements, a.k.a if this is set inactive stop displaying an on hover element
	public static GameObject GetGameObject()
	{
		return instance.gameObject;
	}

	public Transform selectionIndicator;
	public MultiObjectPool mapPools;

	public class UnderMouseData
	{
		public VisitableLocation baseLocation;
		public RealSpacePosition cellCenter;
		public SimulationEntity simulationEntity;
		public float fuelCostToLocation;
	}

	private static UnderMouseData underMouseData = new UnderMouseData();

	public static UnderMouseData GetUnderMouseData()
	{
		return underMouseData;
	}

	private static bool viewRangeOverride = false;
	private const bool mergeEnabled = false;

	[MonitorBreak.Bebug.ConsoleCMD("TrueSight")]
	public static void ToggleViewRangeOverride()
	{
		viewRangeOverride = !viewRangeOverride;
	}

	public static void SetViewRange(float value)
	{
		Shader.SetGlobalFloat("_ShipRange", viewRangeOverride ? 10000.0f : value);
	}

	public static void SetCachedRangeExternal(float value)
	{
		instance.cachedRange = value;
	}

	public static void SetActiveDirectionIndicator(bool _bool)
	{
		instance.playerDirectionIndicator.gameObject.SetActive(_bool);
	}

	[Header("Effects")]
	public Transform targetIcon;
	public Transform rangeIndicator;
	private Material rangeIndicatorMat;
	private float cachedRange;
	public Transform playerDirectionIndicator;
	public LineRenderer indicatorLine;

	[Header("Location Information Disaply")]
	public Canvas canvas;
	private RectTransform canvasRect;
	public RectTransform floatingLocationInformationPopup;
	public TextMeshProUGUI locationTitleLabel;
	public TextMeshProUGUI locationDescLabel;
	public TextMeshProUGUI fuelCostLabel;
	public TextMeshProUGUI compoundLabel;
	public GameObject matchbookDisplay;

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

	public static void FreeezeInteractionCursor(bool active)
	{
		instance.freezeInteractionCursor = active;
	}

	public static void SetActiveMatchBookDisplay(bool active)
	{
		instance.matchbookDisplay.SetActive(active);
	}

	public enum HighlightMode
	{
		None,
		Square,
		Border
	}

	private bool doneInitialDraw;
	private bool freezeInteractionCursor;
	private Dictionary<Vector3, List<LOMCompound>> cellCenterToLocations = new Dictionary<Vector3, List<LOMCompound>>();
	private Dictionary<Transform, SpriteRenderer> transformToLocationRenderer = new Dictionary<Transform, SpriteRenderer>();

	private Func<VisitableLocation, int> getPositionsOperation;

	private void Awake()
	{
		instance = this;
		//Create operations
		getPositionsOperation = delegate (VisitableLocation location) 
		{
			AddPosition(-location.GetPosition().AsTruncatedVector3(MapManagement.mapRelativeScaleModifier), location);

			return 0;
		};
		//

		canvasRect = canvas.transform as RectTransform;
		rangeIndicatorMat = rangeIndicator.GetComponent<MeshRenderer>().material;

		underMouseData = new UnderMouseData();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		rangeIndicator.gameObject.SetActive(false);
		SetViewRange(0.0f);
		doneInitialDraw = false;

		if (!freezeInteractionCursor)
		{
			selectionIndicator.gameObject.SetActive(false);
			targetIcon.gameObject.SetActive(false);
			indicatorLine.positionCount = 0;
		}

		mapPools.HideAllObjects(4);
		mapPools.HideAllObjects(9);

		PlayerCapitalShip.onJumpStart.AddListener(OnJumpStart);
	}

	private void OnDisable()
	{
		PlayerCapitalShip.onJumpStart.RemoveListener(OnJumpStart);
	}

	public void OnJumpStart()
	{
		//Set initial draw back to false so when the player arrives the inital draw is done again
		//Otherwise locations won't show up till next post tick call
		doneInitialDraw = false;

		mapPools.HideAllObjects(4);
		mapPools.HideAllObjects(9);

		indicatorLine.positionCount = 0;
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
		//Normal indicators
		mapPools.PruneObjectsNotUpdatedThisFrame(4);
		//Flashing indicators
		mapPools.PruneObjectsNotUpdatedThisFrame(9, true);
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

		if (PlayerManagement.PlayerEntityExists())
		{
			chunkRange = Mathf.FloorToInt(PlayerManagement.GetStats().GetStat(Stats.jumpRange.ToString()));
		}

		const float buffer = 1;
		float calculatedRange = (chunkRange * density) + buffer;
		cachedRange = calculatedRange;

		rangeIndicatorMat.SetFloat("_Radius", viewRangeOverride ? 10000.0f : calculatedRange);

		PlayerLocationManagement.PerformOperationOnNearbyLocations(WorldManagement.worldCenterPosition, getPositionsOperation, chunkRange);
	}

	private void AddPosition(Vector3 worldPos, VisitableLocation location)
	{
		//Don't want to be able to jump to a super close position
		const double minimumValidDistance = PlayerLocationManagement.normalMaxDistance;
		if (WorldManagement.OffsetFromWorldCenter(location.GetPosition(), Vector3.zero).Magnitude() < minimumValidDistance)
		{
			return;
		}
		//

		Transform newLocationTrans = mapPools.UpdateNextObjectPosition(4, worldPos);
		if (!transformToLocationRenderer.ContainsKey(newLocationTrans))
		{
			transformToLocationRenderer.Add(newLocationTrans, newLocationTrans.GetComponent<SpriteRenderer>());
		}

		transformToLocationRenderer[newLocationTrans].color = location.GetMapColour();

		//Add an additional flashing indicator if this location requires
		if (location.FlashOnMap())
		{
			Transform newFlashTrans = mapPools.UpdateNextObjectPosition(9, worldPos);

			if (!transformToLocationRenderer.ContainsKey(newFlashTrans))
			{
				transformToLocationRenderer.Add(newFlashTrans, newFlashTrans.GetComponent<SpriteRenderer>());
			}

			transformToLocationRenderer[newFlashTrans].color = location.GetFlashColour();
		}

		//Get on map cell center
		Vector3 cellCenter = ClampPositionToGridMap(worldPos);

		//Construct Location On Map
		LocationOnMap newLocation = new LocationOnMap();
		newLocation.actualLocationData = location;
		newLocation.worldPos = worldPos;

		//If this location is very close to another then we need to compound it with others
		LOMCompound mergeTarget = null;
		float minimumDistance = float.MaxValue;

		if (mergeEnabled)
		{
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
		}

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

		Vector3 playerPosOnMap = -WorldManagement.worldCenterPosition.AsTruncatedVector3(MapManagement.mapRelativeScaleModifier);
		rangeIndicator.position = playerPosOnMap + Vector3.up * 0.05f;
		Shader.SetGlobalVector("_WCMapPos", playerPosOnMap);

		playerDirectionIndicator.position = playerPosOnMap;
		playerDirectionIndicator.LookAt(playerPosOnMap - PlayerCapitalShip.GetForward());

		if (MapManagement.MapIntroRunning())
		{
			if (MapManagement.LastFrameOfMapIntro())
			{
				rangeIndicator.gameObject.SetActive(true);
			}

			return;
		}
		else
		{
			//Only do inital draw if the player exists (so not during nation selection)
			if (!doneInitialDraw && PlayerManagement.PlayerEntityExists())
			{
				doneInitialDraw = true;
				DrawAvaliableLocations();
			}

			SetViewRange(cachedRange);
		}

		if (PlayerCapitalShip.IsJumping() || (UIHelper.ElementsUnderMouse().Count > 0 && !freezeInteractionCursor))
		{
			//Don't let them set a new position if we are jumping or if we are currently  moused over a ui object
			targetIcon.gameObject.SetActive(false);
			selectionIndicator.gameObject.SetActive(false);
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

			const float mouseDistanceBuffer = 1;

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

			if (freezeInteractionCursor)
			{
				underMouseData.baseLocation = null;
				underMouseData.simulationEntity = null;
				return;
			}

			bool foundLocation = targetLocation != null;
			bool entityFound = false;
			targetIcon.gameObject.SetActive(foundLocation);
			underMouseData.fuelCostToLocation = 0.0f;

			//Calculate cell center
			//Convert mouse position back to simulation space
			Vector3 onMapPosition = -hitPoint;
			RealSpacePosition simPos = new RealSpacePosition(onMapPosition.x, onMapPosition.y, onMapPosition.z).Multiply(MapManagement.mapRelativeScaleModifier);
			//Clamp to a cell center
			simPos = WorldManagement.ClampPositionToGrid(simPos);

			underMouseData.cellCenter = simPos;
			//

			if (foundLocation)
			{
				//Update global target information
				underMouseData.baseLocation = targetLocation.primaryLocation.actualLocationData;

				EntityLink link = underMouseData.baseLocation.parent;
				if (link != null)
				{
					underMouseData.simulationEntity = underMouseData.baseLocation.parent.Get();
					entityFound = true;
				}
				else
				{
					underMouseData.simulationEntity = null;
				}

				//Display target information
				//Currently just displaying the primary locations information
				targetIcon.position = targetLocation.primaryLocation.worldPos;
				locationTitleLabel.text = targetLocation.primaryLocation.actualLocationData.GetTitle();
				locationDescLabel.text = targetLocation.primaryLocation.actualLocationData.GetDescription();

				//Move window to be below mouse
				RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, canvas.worldCamera, out Vector2 mousePos);
				floatingLocationInformationPopup.position = canvasRect.TransformPoint(mousePos);

				//Calculate fuel cost
				int fuelCost = 0;
                double distance = PlayerCapitalShip.CalculateDistance(targetLocation.primaryLocation.actualLocationData.GetPosition());

                if (PlayerManagement.fuelEnabled) 
				{
                    //Fuel is calculated in this script because this form of travel uses fuel
                    //Some forms of travel won't so it's not done directly in the jump function
                    fuelCost = (int)Math.Floor(distance / 100);
                    //Disaply fuel cost
                    fuelCostLabel.text = fuelCost.ToString();
                }
				else
				{
					//+7 from inital jump animation time
					int dayEstimate = (int)Math.Round((10 * (distance / PlayerCapitalShip.jumpBaseLineSpeed)) / 3.0f) + 7;
					fuelCostLabel.text = $"~{dayEstimate} Days";
				}

				underMouseData.fuelCostToLocation = fuelCost;

				//Display number of locations compounded into this one
				string compoundLabelText = string.Empty;

				if (targetLocation.secondaryLocations.Count > 0)
				{
					compoundLabelText = $"+{targetLocation.secondaryLocations.Count}";
				}

				compoundLabel.text = compoundLabelText;

				//Get inventory
				PlayerInventory playerInventory = PlayerManagement.GetInventory();

                if (!PlayerManagement.fuelEnabled || playerInventory.fuel >= fuelCost)
                {
                    fuelCostLabel.color = Color.green;
                }
                else
                {
                    fuelCostLabel.color = Color.red;
                }
            }
			else
			{
				//Reset
				underMouseData.baseLocation = null;
				underMouseData.simulationEntity = null;

				//Check if territory is within range, shouldn't allow player to select things outside their jump range
				bool withinInteractionRange = true;

				if (PlayerManagement.PlayerEntityExists())
				{
					const float rangeCheckBuffer = 1;

					float distanceToPlayer = Vector3.Distance(playerPosOnMap, hitPoint);
					float maxRangeOnMap = (density * PlayerManagement.GetStats().GetStat(Stats.jumpRange.ToString())) + rangeCheckBuffer;

					withinInteractionRange = distanceToPlayer <= maxRangeOnMap;
				}

				if (withinInteractionRange)
				{
					//Find if over any territory

					List<TerritoryData> territories = SimulationManagement.GetDataViaTag(DataTags.Territory).Cast<TerritoryData>().ToList();
					foreach (TerritoryData territoryData in territories)
					{
						if (territoryData.territoryCenters.Contains(simPos))
						{
							underMouseData.simulationEntity = territoryData.parent.Get();
							entityFound = true;
							break;
						}
					}
				}
			}

			//Decide on highlight mode based on interaction
			Interaction lastSVI = PlayerInteractionManagement.lastSuccesfullyValidatedInteraction;
			Interaction.InteractionMapCursor mapCursorData = Interaction.basicBorder;

			//Don't display highlight effect if over location or no interaction
			entityFound = entityFound && underMouseData.baseLocation == null && lastSVI != null;

			if (lastSVI != null)
			{
				mapCursorData = lastSVI.GetMapCursorData(underMouseData);
			}

			//Square Highlight
			bool showSquare = entityFound && mapCursorData.highlightMode == HighlightMode.Square;
			selectionIndicator.gameObject.SetActive(showSquare);

			if (showSquare)
			{
				//Convert sim pos to map pos
				selectionIndicator.position = ClampPositionToGridMap(hitPoint);
			}
			//

			//Border Highlight
			bool colourBorder = entityFound && mapCursorData.highlightMode == HighlightMode.Border;
			if (colourBorder)
			{
				int id = underMouseData.simulationEntity.id;
				//Adds an override for this frame only
				//Meaning it handles any potential desync from map refresh
				MapManagement.CreateBorderColourOverride(id, Color.white);
			}
			//

			//Indicator line
			if (mapCursorData.showLineIndicator && (entityFound || foundLocation))
			{
				Vector3 targetPos;

				if (foundLocation)
				{
					targetPos = targetLocation.primaryLocation.worldPos;
				}
				else
				{
					targetPos = -underMouseData.cellCenter.AsTruncatedVector3(MapManagement.mapRelativeScaleModifier);
				}


				//Generate a spline from player pos to target
				PathHelper.SimplePath spline = PathHelper.GenerateSimplePathStatic(targetPos, playerPosOnMap, new PathHelper.SimplePathParameters()
				{
					forwardVector = Vector3.up,
					rightVector = Vector3.zero
				});

				float rawDistance = Vector3.Distance(targetPos, playerPosOnMap);
				int res = Mathf.CeilToInt(Mathf.Max(10, 2 * rawDistance));

				Vector3[] positions = new Vector3[res + 1];
				for (int i = 0; i <= res; i++)
				{
					float percentage = i / (float)res;
					positions[i] = spline.GetPosition(percentage);

					positions[i].y += Mathf.Sin(percentage * Mathf.PI) * (Mathf.Max(rawDistance, 1.0f) * 0.25f); //Make shape more exagerated
				}

				indicatorLine.positionCount = res + 1;
				indicatorLine.SetPositions(positions);
			}
			else
			{
				indicatorLine.positionCount = 0;
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
