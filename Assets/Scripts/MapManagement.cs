using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class MapManagement : MonoBehaviour
{
    public TextMeshProUGUI dateLabel;
    public MultiObjectPool mapElementsPools;
    private const int mapRingPool = 0;
    private const int shipIndicatorPool = 1;
    private const int mapBasePool = 2;
	private const bool debugMode = true;

    private List<(Transform, Transform)> mapObjectsAndParents;
    private bool pastFirstFrameOfMapAnim = false;
    private bool extraFrame;
    private Vector3 mapBasePos;

    private float mapRefreshTime;
    private float dateRefreshTime;

    private Dictionary<Transform, MeshRenderer> mapRingMeshRenderes;
    private Dictionary<Transform, LineRenderer> borderRenderers;
    private Dictionary<Transform, SpriteRenderer> nationIconRenderers;
    private Dictionary<Transform, LineRenderer> tradeRouteRenderers;


    public static Vector3 GetDisplayOffset()
    {
        return (new Vector3(0, -1, 1).normalized * CameraManagement.cameraOffsetInMap) +
                        WorldManagement.worldCenterPosition.TruncatedVector3(UIManagement.mapRelativeScaleModifier);
    }

    private void Start()
    {
        mapRingMeshRenderes = mapElementsPools.GetComponentsOnAllActiveObjects<MeshRenderer>(0);

        borderRenderers = mapElementsPools.GetComponentsOnAllActiveObjects<LineRenderer>(3);

        nationIconRenderers = mapElementsPools.GetComponentsOnAllActiveObjects<SpriteRenderer>(5);

        tradeRouteRenderers = mapElementsPools.GetComponentsOnAllActiveObjects<LineRenderer>(6);
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

                    Vector3 scale = Vector3.one * (float)(WorldManagement.GetGridDensity() / UIManagement.mapRelativeScaleModifier);
                    Vector3 displayOffset = GetDisplayOffset();

                    //RUN BORDER IN ORDER ROUTINE
                    //This is a very expensive operation that is currently (08/11/2024) the sole reason behind MAP_REFRESH_ENABLED being set to false
                    //it could almost certainly be optomized in a variety of ways but it runs well enough that for this vertical slice/beta version
                    //it is fine. The focus should be one other things

					//It also does not handle degenerate cases well, in the sense it doesn't handle them at all hahahahaha
                    //SimulationManagement.RunAbsentRoutine("BorderInOrder");

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

									for (int i = 0; i < territoryData.territoryCenters.Count; i++)
									{
										MonitorBreak.Bebug.Helper.DrawWirePlane(
											-territoryData.territoryCenters.ElementAt(i).TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset,
											debugScale,
											Vector3.up,
											factionColour,
											timeTillNextMapUpdate,
											true);
									}


									GameWorld gameworld = (GameWorld)SimulationManagement.GetAllFactionsWithTag(Faction.Tags.GameWorld)[0];
									gameworld.GetData(Faction.Tags.GameWorld, out GlobalBattleData globalBattleData);

									foreach (KeyValuePair<RealSpacePostion, GlobalBattleData.Battle> battle in globalBattleData.battles)
									{
										int wdhjbnhjawbndhjabhjgdb = battle.Value.involvedFactions.Count;

										for (int k = 0; k < wdhjbnhjawbndhjabhjgdb; k++)
										{
											MonitorBreak.Bebug.Helper.DrawWirePlane(
												-battle.Key.TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset,
												debugScale * (1.0f + (k * 0.1f)),
												Vector3.up,
												Color.yellow,
												timeTillNextMapUpdate,
												true);
										}
									}
								}
								else
								{

									Vector3 averagePos = Vector3.zero;
									Transform borderRenderer = mapElementsPools.UpdateNextObjectPosition(3, Vector3.zero);
									LineRenderer lineRenderer = borderRenderers[borderRenderer];

									//Idea is we start at a given border point and we traverse round the border creating a line
									//When faced with multiple options on where to go we split and do both, we want the path that maximises closeness to the original shape
									//In our case "closeness to the original shape" can be defined by how close the number of generated points is too the full number of borders
									//So in esscense we want to maximize the amount of border points we traverse (ideally all of them)

									if (lineRenderer != null)
									{
#pragma warning disable CS0618 // Type or member is obsolete
										lineRenderer.SetColors(factionColour, factionColour);
#pragma warning restore CS0618 // Type or member is obsolete

										if (territoryData.borderInOrder != null)
										{
											//Apply the found path to the line renderer
											count = territoryData.borderInOrder.Count;
											lineRenderer.positionCount = count + 1;

											for (int i = 0; i < count; i++)
											{
												Vector3 pos = territoryData.borderInOrder[i] + displayOffset;

												lineRenderer.SetPosition(i, pos);
												averagePos += pos;
											}

											//Make it loop
											lineRenderer.SetPosition(count, lineRenderer.GetPosition(0));

											if (faction.GetData(Faction.Tags.Emblem, out EmblemData emblemData))
											{
												averagePos /= count;

												Vector3 averageOffset = Vector3.zero;
												float max = 0;

												Vector3[] allPositions = new Vector3[lineRenderer.positionCount];
												lineRenderer.GetPositions(allPositions);

												for (int i = 0; i < count; i++)
												{
													Vector3 offset = allPositions[i] - averagePos;
													averageOffset += offset;

													max = Mathf.Max(max, Mathf.Abs(offset.x), Mathf.Abs(offset.z));
												}

												Transform centralIcon = mapElementsPools.UpdateNextObjectPosition(5, averagePos - averageOffset - (Vector3.up * 0.25f));
												float iconScale = 14 * Mathf.Log(max, 30);

												centralIcon.localScale = Vector3.one * Mathf.Clamp(iconScale, 1, 100);

												if (centralIcon != null)
												{
													nationIconRenderers[centralIcon].sprite = emblemData.icon;
													nationIconRenderers[centralIcon].color = emblemData.mainColour;
												}
											}
										}
									}
								}
                            }
                        }
                    }


					if (SimulationSettings.DrawSettlements())
                    {
						List<Faction> settlements = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Settlements);
						foreach (Faction settlement in settlements)
                        {
                            if (settlement.GetData(Faction.Tags.Settlements, out SettlementData data))
                            {
                                foreach (KeyValuePair<RealSpacePostion, SettlementData.Settlement> s in data.settlements)
                                {
                                    mapElementsPools.UpdateNextObjectPosition(4, -s.Value.actualSettlementPos.TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset);

                                    //Trade trade paths
                                    //foreach (SettlementData.Settlement.TradeFleet tradeFleet in s.Value.tradeFleets)
                                    //{
                                    //    foreach (TradeShip ship in tradeFleet.ships)
                                    //    {
                                    //        if (ship.tradeTarget != null)
                                    //        {
                                    //            LineRenderer renderer = tradeRouteRenderers[mapElementsPools.UpdateNextObjectPosition(6, Vector3.zero)];
                                    //            renderer.positionCount = 2;
                                    //            renderer.SetPosition(0,
                                    //                -ship.homeLocation.GetPosition().TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset);
                                    //            renderer.SetPosition(1,
                                    //                -ship.tradeTarget.GetPosition().TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset);
                                    //        }
                                    //    }
                                    //}
                                }
                            }
                        }
                    }

					if (SimulationSettings.DrawMilitaryPresence())
					{
						//This is a very dirty way of doing this but
						//All of this will be replaced with non debug UI at some point I pray

						List<Faction> mil = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.HasMilitary);

						foreach (Faction military in mil)
						{
							Color factionColour = military.GetColour();

							if (military.GetData(Faction.Tags.HasMilitary, out MilitaryData milData))
							{
								if (military.GetData(Faction.battleDataKey, out BattleData battleData))
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

										Debug.DrawRay(pos + minorOffset, Vector3.up * entry.Value.Count * 2, factionColour, timeTillNextMapUpdate);
										Debug.DrawRay(pos + minorOffset, Vector3.up * entry.Value.Count, color, timeTillNextMapUpdate);
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


