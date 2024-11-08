using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MapManagement : MonoBehaviour
{
    public TextMeshProUGUI dateLabel;
    public MultiObjectPool mapElementsPools;
    private const int mapRingPool = 0;
    private const int shipIndicatorPool = 1;
    private const int mapBasePool = 2;

    private const bool MAP_REFRESH_ENABLED = true;
    private const bool SHOW_SETTLEMENTS = false;

    private List<(Transform, Transform)> mapObjectsAndParents;
    private bool mapObjectsListSetupDone = false;
    private bool extraFrame;
    private Vector3 mapBasePos;

    private float mapRefreshTime;
    private float dateRefreshTime;

    private Dictionary<Transform, MeshRenderer> mapRingMeshRenderes;

    private Dictionary<Transform, MeshRenderer> borderIndicatorRenderers;

    private Dictionary<Transform, SpriteRenderer> nationIconRenderers;

    private void Start()
    {
        mapRingMeshRenderes = mapElementsPools.GetComponentsOnAllActiveObjects<MeshRenderer>(0);

        borderIndicatorRenderers = mapElementsPools.GetComponentsOnAllActiveObjects<MeshRenderer>(3);

        nationIconRenderers = mapElementsPools.GetComponentsOnAllActiveObjects<SpriteRenderer>(5);
    }

    private void OnEnable()
    {
        mapObjectsListSetupDone = false;
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
                if (!mapObjectsListSetupDone)
                {
                    //Do inital date set
                    dateLabel.text = SimulationManagement.GetDateString();

                    //Disable any map elements that might still be showing
                    //The parent object will have been set inactive so they won't actually render
                    //But they would now cause we have turned the object active again
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(3);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(4);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(5);

                    mapObjectsAndParents = new List<(Transform, Transform)>();

                    List<SurroundingObject> surroundingObjects = SurroundingsRenderingManagement.GetControlledObjects();

                    foreach (SurroundingObject surroundingObject in surroundingObjects)
                    {
                        mapObjectsAndParents.Add((surroundingObject.transform, surroundingObject.GetInWorldParent()));
                    }

                    Vector3 shipIndicatorPos = new Vector3(0, -1, 1).normalized * CameraManagement.cameraOffsetInMap;
                    mapElementsPools.UpdateNextObjectPosition(shipIndicatorPool, shipIndicatorPos);

                    mapBasePos = shipIndicatorPos + WorldManagement.worldCenterPosition.TruncatedVector3(UIManagement.mapRelativeScaleModifier);
                    mapObjectsListSetupDone = true;
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

                    Debug.DrawLine(entry.Item2.position, entry.Item2.position + scale, Color.red);
                }

                Vector3 mapBasePosThisFrame = mapBasePos + (Vector3.up * Mathf.Lerp(-25, -2, UIManagement.EvaluatedMapIntroT()));
                mapElementsPools.UpdateNextObjectPosition(mapBasePool, mapBasePosThisFrame);

                mapElementsPools.PruneObjectsNotUpdatedThisFrame(mapRingPool);
                mapElementsPools.PruneObjectsNotUpdatedThisFrame(mapBasePool);
            }
            else
            {
                if (Time.time > mapRefreshTime && (MAP_REFRESH_ENABLED || mapRefreshTime == 0))
                {
                    mapRefreshTime = Time.time + (5.0f / SimulationManagement.GetSimulationSpeed());
                    //We also want to steup the current territory borders here cause the intro animation is now done
                    //We need to get all the factions with the territory tag and then spawn territory squares based on that
                    List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Territory);

                    Vector3 scale = Vector3.one * (float)(WorldManagement.GetGridDensity() / UIManagement.mapRelativeScaleModifier);
                    Vector3 displayOffset = (new Vector3(0, -1, 1).normalized * CameraManagement.cameraOffsetInMap) + WorldManagement.worldCenterPosition.TruncatedVector3(UIManagement.mapRelativeScaleModifier);

                    foreach (Faction faction in factions)
                    {
                        if (faction.GetData(Faction.Tags.Territory, out TerritoryData territoryData))
                        {
                            Color factionColour = faction.GetColour();

                            Vector3 averagePos = Vector3.zero;
                            List<Vector3> positions = new List<Vector3>();
                            int count = territoryData.territoryCenters.Count;

                            if (count > 0)
                            {
                                foreach (RealSpacePostion pos in territoryData.territoryCenters)
                                {
                                    Vector3 truncPos = -pos.TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset;
                                    Transform newPiece = mapElementsPools.UpdateNextObjectPosition(3, truncPos);

                                    if (newPiece != null)
                                    {
                                        borderIndicatorRenderers[newPiece].material.SetColor("_Colour", factionColour);
                                        newPiece.localScale = scale;
                                    }

                                    averagePos += truncPos;
                                    positions.Add(truncPos);
                                }
                            }

                            if (faction.GetData(Faction.Tags.Emblem, out EmblemData emblemData))
                            {
                                averagePos /= count;
                                //This is expensive but the effect looks worse without it
                                //If we want to optomize we should store all the borders for the territories
                                Vector3 averageOffset = Vector3.zero;
                                float max = 0;

                                foreach (Vector3 pos in positions)
                                {
                                    Vector3 offset = pos - averagePos;
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

                    if (SHOW_SETTLEMENTS)
                    {
                        List<Faction> settlements = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Settlements);

                        foreach (Faction settlement in settlements)
                        {
                            if (settlement.GetData(Faction.Tags.Settlements, out SettlementData data))
                            {
                                foreach (KeyValuePair<RealSpacePostion, SettlementData.Settlement> s in data.settlements)
                                {
                                    mapElementsPools.UpdateNextObjectPosition(4, -s.Value.actualSettlementPos.TruncatedVector3(UIManagement.mapRelativeScaleModifier) + displayOffset);
                                }
                            }
                        }
                    }

                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(3);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(4);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(5);
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
