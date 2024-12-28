using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class MapManagement : UIState
{
	/// UI STATE IMPLEMENTATION
	private static MapManagement instance;
    public const float mapRelativeScaleModifier = 1000.0f;
    public AnimationCurve mapIntroCurve;

    public override float GetIntroSpeed()
    {
        return 1.5f;
    }

    public override KeyCode GetSetActiveKey()
    {
        return InputManagement.toggleMapKey;
    }

    public static bool MapIntroRunning()
	{
		return instance.IntroRunning();
	}

	public static bool FirstFrameMapIntroRunning()
	{
		return instance.FirstFrameOfIntro();
	}

	public static bool LastFrameOfMapIntro()
	{
		return instance.LastFrameOfIntro();
	}

	public static float EvaluatedMapIntroT()
	{
		return instance.mapIntroCurve.Evaluate(1.0f - instance.GetCurrentIntroT());
	}

	public static bool MapActive()
	{
		return instance.GetTargetObject().activeSelf;
	}

    protected override void OnSetActive(bool _bool)
    {
        base.OnSetActive(_bool);

		CameraManagement.SetMainCameraActive(!_bool);
        SurroundingsRenderingManagement.SetActivePlanetLighting(!_bool);
    }

    ///
    public class TroopTransferEffect
	{
		public LineRenderer target;
		public PathHelper.SimplePath path;
		public int pathResolution;
		public float startTime;
		public float length;

		public float Update(AnimationCurve animationCurve)
		{
			float timePercentage = Mathf.Clamp01((Time.time - startTime) / length);
			
			float min = 0;//Mathf.Clamp01((timePercentage * 2.0f) - 1.0f);
			float max = animationCurve.Evaluate(Mathf.Clamp01(timePercentage * 2.0f));

			Vector3[] newLinePositions = new Vector3[pathResolution];
			for (int i = 0; i <= pathResolution-1; i++)
			{
				newLinePositions[i] = path.GetPosition(Mathf.Clamp01(min + ((i / (float)(pathResolution-1)) * Mathf.Clamp01(max - min))));
			}

			target.SetPositions(newLinePositions);

			return timePercentage;
		}
	}

	[Header("Map Settings")]
    public MultiObjectPool mapElementsPools;
    private const int mapRingPool = 0;
    private const int shipIndicatorPool = 1;
    private const int mapBasePool = 2;
	private const bool debugMode = false;

    private List<(Transform, Transform)> mapObjectsAndParents;
    private bool pastFirstFrameOfMapAnim = false;
    private bool extraFrame;
    private Vector3 mapBasePos;

    private float mapRefreshTime;

    private Dictionary<Transform, MeshRenderer> mapRingMeshRenderes;
	private Dictionary<Transform, LineRenderer> cachedTransformToBorderRenderer = new Dictionary<Transform, LineRenderer>();
	private Dictionary<Transform, SpriteRenderer> cachedTransformToNationIconRenderers = new Dictionary<Transform, SpriteRenderer>();
	private List<TroopTransferEffect> ttEffects = new List<TroopTransferEffect>();

	public AnimationCurve troopTransferCurve;

    protected override void Awake()
    {
		instance = this;
        base.Awake();
    }

    private void Start()
    {
        mapRingMeshRenderes = mapElementsPools.GetComponentsOnAllActiveObjects<MeshRenderer>(0);
    }

    private void OnEnable()
    {
        pastFirstFrameOfMapAnim = false;
        extraFrame = true;
    }

    private void Update()
    {
        if (MapActive())
        {
			Vector3 playerPos = -PlayerCapitalShip.GetPCSPosition().AsTruncatedVector3(mapRelativeScaleModifier);
			mapElementsPools.UpdateNextObjectPosition(shipIndicatorPool, playerPos);
			mapElementsPools.PruneObjectsNotUpdatedThisFrame(shipIndicatorPool);

			if (MapIntroRunning() || extraFrame)
            {
                if (!MapIntroRunning())
                {
					//Extra frame after map intro runs

                    //Want to run for an extra frame otherwise surroundings rendering could move out of sync after we've finished
                    //This actually happens a lot because of the speed of the map intro animation
                    extraFrame = false;
                    Shader.SetGlobalFloat("_FlashTime", Time.time);
                    mapRefreshTime = 0.0f;
                }

				//Can't use UIManagment's first frame of intro anim because we get set active a frame after the intro anim starts :(
                if (!pastFirstFrameOfMapAnim)
                {
					//Disable any map elements that might still be showing
					//The parent object will have been set inactive so they won't actually render
					//But they would now cause we have turned the object active again
					for (int i = 3; i <= 8; i++)
					{
						if (i == 4)
						{
							continue;
						}

						mapElementsPools.PruneObjectsNotUpdatedThisFrame(i);
					}

					mapObjectsAndParents = new List<(Transform, Transform)>();

                    List<SurroundingObject> surroundingObjects = SurroundingsRenderingManagement.GetControlledObjects();

                    foreach (SurroundingObject surroundingObject in surroundingObjects)
                    {
                        mapObjectsAndParents.Add((surroundingObject.transform, surroundingObject.GetInWorldParent()));
                    };

                    mapBasePos = Vector3.zero;
                }

                foreach ((Transform, Transform) entry in mapObjectsAndParents)
                {
                    Vector3 parentPos = Vector3.zero;

                    if (entry.Item2 != null)
                    {
                        parentPos = entry.Item2.position;
                    }

                    parentPos.y = entry.Item1.position.y;
                    Vector3 scale = entry.Item1.position - parentPos;
                    scale.y = 0;
                    float scaleMag = Mathf.Max(1, scale.magnitude);

                    Transform mapRing = mapElementsPools.UpdateNextObjectPosition(mapRingPool, parentPos);
                    mapRing.localScale = new Vector3(scaleMag, 1, scaleMag) * 2;
                    mapRingMeshRenderes[mapRing].material.SetFloat("_Radius", scaleMag);
                    mapRingMeshRenderes[mapRing].material.SetVector("_RingItemPos", entry.Item1.position);
                    mapRingMeshRenderes[mapRing].material.SetFloat("_RingItemRadius", entry.Item1.localScale.magnitude * 0.75f);

					if (!pastFirstFrameOfMapAnim)
					{
						mapRingMeshRenderes[mapRing].material.SetFloat("_Thickness", Mathf.Max(0.1f, 0.3f * entry.Item1.localScale.x));
					}
                }

                Vector3 mapBasePosThisFrame = mapBasePos + (Vector3.up * Mathf.Lerp(-25, -0.1f, EvaluatedMapIntroT()));
                mapElementsPools.UpdateNextObjectPosition(mapBasePool, mapBasePosThisFrame);

                mapElementsPools.PruneObjectsNotUpdatedThisFrame(mapRingPool);
                mapElementsPools.PruneObjectsNotUpdatedThisFrame(mapBasePool);
				pastFirstFrameOfMapAnim = true;
			}
            else
            {
				if (PlayerCapitalShip.IsJumping())
				{
					//Update journey indicator
					Vector3 playerTargetPos = -PlayerCapitalShip.GetTargetPosition().AsTruncatedVector3(mapRelativeScaleModifier);

					//Find displacement
					Vector3 displacement = playerTargetPos - playerPos;

					//Division by 10 = division by (5 * 2). 5 is the unit scale for a plane
					const float buffer = 0.1f;
					float length = (displacement.magnitude / 10);

					//Don't display once close to destination so journey indicator doesn't flip (visually break)
					if (length > buffer)
					{
						Transform journeyLineIndicator = mapElementsPools.UpdateNextObjectPosition(7, playerPos + (displacement / 2.0f) + (Vector3.up * 0.001f));
						journeyLineIndicator.localScale = new Vector3(0.05f, 1, length - buffer);
						journeyLineIndicator.LookAt(playerPos);
					}

					mapElementsPools.UpdateNextObjectPosition(8, playerTargetPos);
				}

				mapElementsPools.PruneObjectsNotUpdatedThisFrame(7);
				mapElementsPools.PruneObjectsNotUpdatedThisFrame(8);

				if (Time.time > mapRefreshTime && (SimulationSettings.UpdateMap() || mapRefreshTime == 0))
                {
					ttEffects.Clear();
					float timeTillNextMapUpdate = (5.0f / SimulationManagement.GetSimulationSpeed());

					mapRefreshTime = Time.time + timeTillNextMapUpdate;
                    //We also want to steup the current territory borders here cause the intro animation is now done
                    //We need to get all the factions with the territory tag and then spawn territory squares based on that
                    List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Territory);

					GameWorld gameworld = (GameWorld)SimulationManagement.GetAllFactionsWithTag(Faction.Tags.GameWorld)[0];
					gameworld.GetData(Faction.Tags.GameWorld, out GlobalBattleData globalBattleData);
					gameworld.GetData(Faction.Tags.Historical, out HistoryData historyData);

					//Draw per faction data
					foreach (Faction faction in factions)
					{
						Color factionColour = faction.GetColour();

						if (faction.GetData(Faction.Tags.Territory, out TerritoryData territoryData))
                        {
                            int count = territoryData.borders.Count;

                            if (count > 0)
                            {
								if (debugMode)
								{
									Vector3 debugScale = new Vector3(1, 0, 1) * (float)(WorldManagement.GetGridDensity() / mapRelativeScaleModifier) * 0.8f;

									for (int i = 0; i < territoryData.borders.Count; i++)
									{
										MonitorBreak.Bebug.Helper.DrawWirePlane(
											-territoryData.borders.ElementAt(i).AsTruncatedVector3(mapRelativeScaleModifier),
											debugScale,
											Vector3.up,
											factionColour,
											timeTillNextMapUpdate,
											true);
									}

									foreach (KeyValuePair<RealSpacePostion, GlobalBattleData.Battle> battle in globalBattleData.battles)
									{
										int wdhjbnhjawbndhjabhjgdb = battle.Value.GetInvolvedFactions().Count;

										for (int k = 0; k < wdhjbnhjawbndhjabhjgdb; k++)
										{
											MonitorBreak.Bebug.Helper.DrawWirePlane(
												-battle.Key.AsTruncatedVector3(mapRelativeScaleModifier),
												debugScale * (1.0f + (k * 0.1f)),
												Vector3.up,
												Color.yellow,
												timeTillNextMapUpdate,
												true);
										}
									}

									foreach (KeyValuePair<RealSpacePostion, HistoryData.HistoryCell> historicalTerritory in historyData.previouslyOwnedTerritories)
									{
										MonitorBreak.Bebug.Helper.DrawWirePlane(
											-historicalTerritory.Key.AsTruncatedVector3(mapRelativeScaleModifier),
											debugScale,
											Vector3.up,
											new Color(0.1f, 0.1f, 0.1f, 1.0f),
											timeTillNextMapUpdate,
											true);
									}
								}
								else
								{
									//New method
									List<List<Vector3>> borderLines = territoryData.CalculateMapBorderPositions(out Vector3 iconPos, out Vector3 iconScale);

									foreach (List<Vector3> line in borderLines)
									{
										Transform borderRendererTransform = mapElementsPools.UpdateNextObjectPosition(3, Vector3.zero);

										if (!cachedTransformToBorderRenderer.ContainsKey(borderRendererTransform))
										{
											cachedTransformToBorderRenderer.Add(borderRendererTransform, borderRendererTransform.GetComponent<LineRenderer>());
										}

										LineRenderer lineRenderer = cachedTransformToBorderRenderer[borderRendererTransform];

										if (lineRenderer != null)
										{
#pragma warning disable CS0618 // Type or member is obsolete
											lineRenderer.SetColors(factionColour, factionColour);
#pragma warning restore CS0618 // Type or member is obsolete

											lineRenderer.positionCount = line.Count;
											lineRenderer.SetPositions(line.ToArray());

											lineRenderer.loop = true;
										}
									}

									if (faction.GetData(Faction.Tags.Emblem, out EmblemData emblemData))
									{
										Transform centralIcon = mapElementsPools.UpdateNextObjectPosition(5, iconPos - (Vector3.up * 0.25f));
										centralIcon.localScale = iconScale;

										if (!cachedTransformToNationIconRenderers.ContainsKey(centralIcon))
										{
											cachedTransformToNationIconRenderers.Add(centralIcon, centralIcon.GetComponent<SpriteRenderer>());
										}

										cachedTransformToNationIconRenderers[centralIcon].sprite = emblemData.icon;
										cachedTransformToNationIconRenderers[centralIcon].color = emblemData.mainColour;
									}
								}
                            }
						}

						if (SimulationSettings.DrawMilitaryPresence())
						{
							PathHelper.SimplePathParameters pathParams = new PathHelper.SimplePathParameters();

							if (faction.GetData(Faction.Tags.HasMilitary, out MilitaryData milData))
							{
								if (debugMode)
								{
									if (faction.GetData(Faction.battleDataKey, out BattleData battleData))
									{
										foreach (KeyValuePair<RealSpacePostion, List<ShipCollection>> entry in milData.cellCenterToFleets)
										{
											Vector3 pos = -entry.Key.AsTruncatedVector3(mapRelativeScaleModifier);

											Color color = Color.green;
											if (battleData.ongoingBattles.ContainsKey(entry.Key))
											{
												color = Color.red;
											}

											Vector3 minorOffset = Random.onUnitSphere;
											minorOffset.y = 0;
											minorOffset.Normalize();
											minorOffset *= 0.1f;

											int shipCount = 0;

											foreach (ShipCollection collection in entry.Value)
											{
												shipCount += collection.GetShips().Count;
											}

											Debug.DrawRay(pos + minorOffset, Vector3.up * shipCount * 2, factionColour, timeTillNextMapUpdate);
											Debug.DrawRay(pos + minorOffset, Vector3.up * shipCount, color, timeTillNextMapUpdate);
										}
									}
								}
								else
								{
									foreach ((RealSpacePostion, RealSpacePostion) entry in milData.markedTransfers)
									{
										Transform borderRendererTransform = mapElementsPools.UpdateNextObjectPosition(3, Vector3.zero);

										if (!cachedTransformToBorderRenderer.ContainsKey(borderRendererTransform))
										{
											cachedTransformToBorderRenderer.Add(borderRendererTransform, borderRendererTransform.GetComponent<LineRenderer>());
										}

										LineRenderer lineRenderer = cachedTransformToBorderRenderer[borderRendererTransform];

										if (lineRenderer != null)
										{
#pragma warning disable CS0618 // Type or member is obsolete
											lineRenderer.SetColors(factionColour, factionColour);
#pragma warning restore CS0618 // Type or member is obsolete

											Vector3 startPos = -entry.Item1.AsTruncatedVector3(mapRelativeScaleModifier);
											RealSpacePostion endPosRS;

											//Draw to actual battle pos
											if (globalBattleData.battles.ContainsKey(entry.Item2))
											{
												 endPosRS = globalBattleData.battles[entry.Item2].GetPosition();
											}
                                            else
                                            {
												endPosRS = entry.Item2;
                                            }

                                            Vector3 endPos = -endPosRS.AsTruncatedVector3(mapRelativeScaleModifier);

											Vector3 difference = endPos - startPos;
											pathParams.forwardVector = difference.normalized;
											pathParams.rightVector = Vector3.up * (difference.magnitude * 0.4f);

											PathHelper.SimplePath path = PathHelper.GenerateSimplePathStatic(startPos, endPos, pathParams);
											int res = Mathf.CeilToInt(path.EstimateLength() / 2.5f);
											res = Mathf.Max(4, res);

											lineRenderer.loop = false;
											lineRenderer.positionCount = res;

                                            TroopTransferEffect newTTE = new TroopTransferEffect
                                            {
                                                length = timeTillNextMapUpdate * Random.Range(0.1f, 0.2f),
                                                path = path,
												pathResolution = res,
                                                startTime = Time.time,
                                                target = lineRenderer
                                            };
                                            ttEffects.Add(newTTE);
										}
									}
								}
							}
						}
					}

                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(3);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(5);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(6);
				}

				for (int i = 0; i < ttEffects.Count;)
				{
					if(ttEffects[i].Update(troopTransferCurve) >= 1.0f)
					{
						ttEffects.RemoveAt(i);
					}
					else
					{
						i++;
					}
				}
            }
        }
    }
}


