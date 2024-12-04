using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using static GlobalBattleData;
using static SettlementData;
using System.Linq.Expressions;

public class MapManagement : MonoBehaviour
{
    public TextMeshProUGUI dateLabel;
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
    private float dateRefreshTime;

    private Dictionary<Transform, MeshRenderer> mapRingMeshRenderes;
	private Dictionary<Transform, LineRenderer> cachedTransformToBorderRenderer = new Dictionary<Transform, LineRenderer>();
    private Dictionary<Transform, SpriteRenderer> cachedTransformToNationIconRenderers = new Dictionary<Transform, SpriteRenderer>();


    public static Vector3 GetDisplayOffset()
    {
        return (new Vector3(0, -1, 1).normalized * CameraManagement.cameraOffsetInMap) +
                        WorldManagement.worldCenterPosition.TruncatedVector3(UIManagement.mapRelativeScaleModifier);
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
        if (UIManagement.MapActive())
        {
            if (UIManagement.MapIntroRunning() || extraFrame)
            {
                if (!UIManagement.MapIntroRunning())
                {
                    //Want to run for an extra frame otherwise surroundings rendering could move out of sync after we've finished
                    //This actually happens a lot because of the speed of the map intro animation
                    extraFrame = false;
                    Shader.SetGlobalFloat("_FlashTime", Time.time);
                    mapRefreshTime = 0.0f;
                    dateRefreshTime = 0.0f;
                }

				//Can't use UIManagment's first frame of intro anim because we get set active a frame after the intro anim starts :(
                if (!pastFirstFrameOfMapAnim)
                {
                    //Do inital date set
                    dateLabel.text = SimulationManagement.GetDateString();

                    //Disable any map elements that might still be showing
                    //The parent object will have been set inactive so they won't actually render
                    //But they would now cause we have turned the object active again
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(3);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(4);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(5);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(6);

                    mapObjectsAndParents = new List<(Transform, Transform)>();

                    List<SurroundingObject> surroundingObjects = SurroundingsRenderingManagement.GetControlledObjects();

                    foreach (SurroundingObject surroundingObject in surroundingObjects)
                    {
                        mapObjectsAndParents.Add((surroundingObject.transform, surroundingObject.GetInWorldParent()));
                    }

                    Vector3 shipIndicatorPos = new Vector3(0, -1, 1).normalized * CameraManagement.cameraOffsetInMap;
                    mapElementsPools.UpdateNextObjectPosition(shipIndicatorPool, shipIndicatorPos);

                    mapBasePos = shipIndicatorPos + WorldManagement.worldCenterPosition.TruncatedVector3(UIManagement.mapRelativeScaleModifier);
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

                Vector3 mapBasePosThisFrame = mapBasePos + (Vector3.up * Mathf.Lerp(-25, -2, UIManagement.EvaluatedMapIntroT()));
                mapElementsPools.UpdateNextObjectPosition(mapBasePool, mapBasePosThisFrame);

                mapElementsPools.PruneObjectsNotUpdatedThisFrame(mapRingPool);
                mapElementsPools.PruneObjectsNotUpdatedThisFrame(mapBasePool);
				pastFirstFrameOfMapAnim = true;
			}
            else
            {
                if (Time.time > mapRefreshTime && (SimulationSettings.UpdateMap() || mapRefreshTime == 0))
                {
					float timeTillNextMapUpdate = (5.0f / SimulationManagement.GetSimulationSpeed());

					mapRefreshTime = Time.time + timeTillNextMapUpdate;
                    //We also want to steup the current territory borders here cause the intro animation is now done
                    //We need to get all the factions with the territory tag and then spawn territory squares based on that
                    List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Territory);

                    Vector3 displayOffset = GetDisplayOffset();

					GameWorld gameworld = (GameWorld)SimulationManagement.GetAllFactionsWithTag(Faction.Tags.GameWorld)[0];
					gameworld.GetData(Faction.Tags.GameWorld, out GlobalBattleData globalBattleData);
					gameworld.GetData(Faction.Tags.Historical, out HistoryData historyData);

					//Draw battle indicators
					foreach (KeyValuePair<RealSpacePostion, Battle> battle in globalBattleData.battles)
					{
						mapElementsPools.UpdateNextObjectPosition(6, -battle.Key.TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset + Vector3.down * 0.1f);
					}

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
									Vector3 debugScale = new Vector3(1, 0, 1) * (float)(WorldManagement.GetGridDensity() / UIManagement.mapRelativeScaleModifier) * 0.8f;

									for (int i = 0; i < territoryData.borders.Count; i++)
									{
										MonitorBreak.Bebug.Helper.DrawWirePlane(
											-territoryData.borders.ElementAt(i).TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset,
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
												-battle.Key.TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset,
												debugScale * (1.0f + (k * 0.1f)),
												Vector3.up,
												Color.yellow,
												timeTillNextMapUpdate,
												true,
												Mathf.Max(0.05f, battle.Value.GetWinProgress(k) / 1.2f));
										}
									}

									foreach (KeyValuePair<RealSpacePostion, HistoryData.HistoryCell> historicalTerritory in historyData.previouslyOwnedTerritories)
									{
										MonitorBreak.Bebug.Helper.DrawWirePlane(
											-historicalTerritory.Key.TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset,
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
									List<List<Vector3>> borderLines = territoryData.CalculateMapBorderPositions(displayOffset, out Vector3 iconPos, out Vector3 iconScale);

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

						if (SimulationSettings.DrawSettlements())
						{
							if (faction.GetData(Faction.Tags.Settlements, out SettlementData data))
							{
								foreach (KeyValuePair<RealSpacePostion, SettlementData.Settlement> s in data.settlements)
								{
									mapElementsPools.UpdateNextObjectPosition(4, -s.Value.actualSettlementPos.TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset);
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
											Vector3 pos = -entry.Key.TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset;

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
											lineRenderer.SetColors(Color.black, factionColour);
#pragma warning restore CS0618 // Type or member is obsolete

											Vector3 startPos = -entry.Item1.TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset;
											Vector3 endPos = -entry.Item2.TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset;

											Vector3 difference = endPos - startPos;
											pathParams.forwardVector = difference.normalized;
											pathParams.rightVector = Vector3.up * (difference.magnitude * 0.4f);

											PathHelper.SimplePath simplePath = PathHelper.GenerateSimplePathStatic(startPos, endPos, pathParams);

											Vector3[] newLinePositions = new Vector3[11];
											for (int i = 0; i <= 10; i++)
											{
												newLinePositions[i] = simplePath.GetPosition(i / 10.0f);
											}

											lineRenderer.loop = false;
											lineRenderer.positionCount = 11;
											lineRenderer.SetPositions(newLinePositions);
										}
									}
								}
							}
						}
					}

                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(3);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(4);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(5);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(6);
				}

                if (Time.time > dateRefreshTime)
                {
                    dateLabel.text = SimulationManagement.GetDateString();
                    dateRefreshTime = Time.time + (1.0f / SimulationManagement.GetSimulationSpeed());
                }
            }
        }
    }
}


