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

	[Header("Location Information Disaply")]
	public Canvas canvas;
	private RectTransform canvasRect;
	public RectTransform floatingLocationInformationPopup;
	public TextMeshProUGUI locationTitleLabel;
	public TextMeshProUGUI locationDescLabel;
	public TextMeshProUGUI fuelCostLabel;

	private class LocationOnMap
	{
		public Vector3 worldPos;
		public VisitableLocation actualLocationData;
	}

	private bool doneInitialDraw;
	private Dictionary<Vector3, List<LocationOnMap>> cellCenterToLocations = new Dictionary<Vector3, List<LocationOnMap>>();
	private Dictionary<Transform, SpriteRenderer> transformToLocationRenderer = new Dictionary<Transform, SpriteRenderer>();

	private void Awake()
	{
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

		List<Faction> allFactions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Faction);
		Dictionary<int, SettlementData> idToSetData = SimulationManagement.GetDataForFactionsList<SettlementData>(allFactions, Faction.Tags.Settlements.ToString());
		Dictionary<int, CapitalData> idToCapitalData = SimulationManagement.GetDataForFactionsList<CapitalData>(allFactions, Faction.Tags.Capital.ToString());

		int chunkRange = 10;

		List<Faction> players = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);

		if (players.Count > 0)
		{
			players[0].GetData(PlayerFaction.statDataKey, out PlayerStats playerStats);
			chunkRange = Mathf.FloorToInt(playerStats.GetStat(Stats.jumprange.ToString()));
		}

		const float buffer = 1;
		float calculatedRange = (chunkRange * density) + buffer;
		cachedRange = calculatedRange;

		rangeIndicatorMat.SetFloat("_Radius", calculatedRange);

		VisitableLocation currentPlayerLocation = PlayerLocationManagement.GetCurrentLocation();
		RealSpacePostion playerPos = currentPlayerLocation.GetPosition();
		RealSpacePostion currentPlayerCellCenter = WorldManagement.ClampPositionToGrid(playerPos);

		double gridDensity = WorldManagement.GetGridDensity();
		//Get the players offset from their cell center
		//The range check works on a cell basis but jump distance should be measured from player's actual position
		//So the x and z need to be offset so we can calculate correctly
		float playerXOffsetFromCellCenter = (float)((currentPlayerCellCenter.x - playerPos.x) / gridDensity);
		float playerZOffsetFromCellCenter = (float)((currentPlayerCellCenter.z - playerPos.z) / gridDensity);

		int bufferedChunkRange = chunkRange + 1;

		for (int x = -bufferedChunkRange; x <= bufferedChunkRange; x++)
		{
			for (int z = -bufferedChunkRange; z <= bufferedChunkRange; z++)
			{
				//Within circle
				if (Mathf.Abs(x + playerXOffsetFromCellCenter) + Mathf.Abs(z + playerZOffsetFromCellCenter) <= bufferedChunkRange)
				{
					//Find offset from cell center
					RealSpacePostion currentCellCenter = new RealSpacePostion(
						currentPlayerCellCenter.x + (gridDensity * x), 
						0, 
						currentPlayerCellCenter.z + (gridDensity * z));

					//Iterate through all factions
					//If they have a location in this chunk place it on the map
					foreach (Faction faction in allFactions)
					{
						//Settlements
						if (idToSetData.ContainsKey(faction.id))
						{
							SettlementData settlementData = idToSetData[faction.id];
							if (settlementData.settlements.ContainsKey(currentCellCenter))
							{
								SettlementData.Settlement settlement = settlementData.settlements[currentCellCenter];
								Vector3 pos = -settlement.actualSettlementPos.AsTruncatedVector3(UIManagement.mapRelativeScaleModifier);

								AddPosition(pos, settlement.location);
							}
						}
						//

						//Capitals
						if (idToCapitalData.ContainsKey(faction.id))
						{
							CapitalData capitalData = idToCapitalData[faction.id];
							if (capitalData.position.Equals(currentCellCenter))
							{
								Vector3 pos = -capitalData.position.AsTruncatedVector3(UIManagement.mapRelativeScaleModifier);

								AddPosition(pos, capitalData.location);
							}
						}
						//
					}
				}
			}
		}
	}

	private void AddPosition(Vector3 worldPos, VisitableLocation location)
	{
		if (PlayerLocationManagement.GetCurrentLocation().Equals(location))
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

		if (!cellCenterToLocations.ContainsKey(cellCenter))
		{
			cellCenterToLocations.Add(cellCenter, new List<LocationOnMap>());
		}

		LocationOnMap newLocation = new LocationOnMap();
		newLocation.actualLocationData = location;
		newLocation.worldPos = worldPos;

		cellCenterToLocations[cellCenter].Add(newLocation);
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

			LocationOnMap targetLocation = null;
			float currentShortestDistance = float.MaxValue;

			const float mouseDistanceBuffer = 2;
			const int checkDimensions = 1;

			Vector3 cellCenter = ClampPositionToGridMap(hitPoint);

			for (int x = -checkDimensions; x <= checkDimensions; x++)
			{
				for (int z = -checkDimensions; z <= checkDimensions; z++)
				{
					//Calculate cell center to check
					Vector3 currentCellCenter = cellCenter + (new Vector3(x, 0, z) * density);

					if (cellCenterToLocations.ContainsKey(currentCellCenter))
					{
						//Any locations in this cell
						foreach (LocationOnMap locationOnMap in cellCenterToLocations[currentCellCenter])
						{
							float sqrDistance = (locationOnMap.worldPos - hitPoint).sqrMagnitude;

							if (sqrDistance <= mouseDistanceBuffer && sqrDistance < currentShortestDistance)
							{
								currentShortestDistance = sqrDistance;
								targetLocation = locationOnMap;
							}
						}
					}
				}
			}

			bool foundLocation = targetLocation != null;
			targetIcon.gameObject.SetActive(foundLocation);

			if (foundLocation)
			{
				//Disaply target information
				targetIcon.position = targetLocation.worldPos;
				locationTitleLabel.text = targetLocation.actualLocationData.GetTitle();
				locationDescLabel.text = targetLocation.actualLocationData.GetDescription();

				//Move window to be below mouse
				RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, canvas.worldCamera, out Vector2 mousePos);
				floatingLocationInformationPopup.position = canvasRect.TransformPoint(mousePos);

				//Calculate fuel cost
				int fuelCost;

				//Fuel is calculated in this script because this form of travel uses fuel
				//Some forms of travel won't so it's not done directly in the jump function
				double distance = PlayerCapitalShip.CalculateDistance(targetLocation.actualLocationData.GetPosition());
				fuelCost = (int)Math.Floor(distance / 100);

				//Disaply fuel cost
				fuelCostLabel.text = fuelCost.ToString();

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
							PlayerCapitalShip.StartJump(targetLocation.actualLocationData);

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
}
