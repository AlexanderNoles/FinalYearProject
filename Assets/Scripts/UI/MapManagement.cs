using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using EntityAndDataDescriptor;

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
        SurroundingsRenderingManagement.SetNotInMap(!_bool);

		if (_bool)
        {
            extraFrame = true;
			pastFirstFrameOfMapAnim = false;
            //Disable any map elements that might still be showing
            //The parent object will have been set inactive so they won't actually render
            //But they would now cause we have turned the object active again
            for (int i = 3; i <= 8; i++)
            {
                if (i == 4)
                {
                    continue;
                }

                mapElementsPools.HideAllObjects(i);
            }

            mapObjectsAndParents = new List<(Transform, Transform)>();

            List<SurroundingObject> surroundingObjects = SurroundingsRenderingManagement.GetControlledObjects();

            foreach (SurroundingObject surroundingObject in surroundingObjects)
            {
                mapObjectsAndParents.Add((surroundingObject.transform, surroundingObject.GetInWorldParent()));
            };

            mapBasePos = Vector3.zero;
        }
		else
		{
			cantFindLocation.SetActive(false);
		}
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
	public GameObject cantFindLocation;
	public FadeOnEnable fadeInEffect;
    public MultiObjectPool mapElementsPools;
    private const int mapRingPool = 0;
    private const int shipIndicatorPool = 1;
    private const int mapBasePool = 2;

    private List<(Transform, Transform)> mapObjectsAndParents;
    private bool pastFirstFrameOfMapAnim = false;
    private bool extraFrame;
    private Vector3 mapBasePos;

    private float mapRefreshTime;
    private const bool autoUpdateMap = true;

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

    private void Update()
    {
        if (MapActive())
        {
			if (PlayerCapitalShip.InJumpTravelStage())
			{
				if (!cantFindLocation.activeSelf)
				{
					//Restart fade effect so it still plays even if we are in the map
					//Don't use a second effect for just can't find location ui
					//as they would overlap and look messy
					//It's also kinda wasteful
					fadeInEffect.Restart();
					cantFindLocation.SetActive(true);
				}

				return;
			}
			else if (cantFindLocation.activeSelf)
			{
				fadeInEffect.Restart();
				cantFindLocation.SetActive(false);
			}

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

				mapElementsPools.PruneObjectsNotUpdatedThisFrame(7, true);
				mapElementsPools.PruneObjectsNotUpdatedThisFrame(8, true);

                const bool drawInformationOnMap = true;

				if (drawInformationOnMap && (Time.time > mapRefreshTime && (autoUpdateMap || mapRefreshTime == 0)))
                {
                    GameWorld.main.GetData(DataTags.GlobalBattle, out GlobalBattleData globalBattleData);
                    GameWorld.main.GetData(DataTags.Historical, out GlobalBattleData historyData);
                    //Get all emblem datas so they can be accessed later on
                    Dictionary<int, EmblemData> entityIDtoEmblemDatas = SimulationManagement.GetEntityIDToData<EmblemData>(DataTags.Emblem);

                    ttEffects.Clear();
					float timeTillNextMapUpdate = (5.0f / SimulationManagement.GetSimulationSpeed());

					mapRefreshTime = Time.time + timeTillNextMapUpdate;
					//We want to draw the current territory borders now cause the intro animation is done
					//We need to get all the territory data modules and then draw lines based on them
					List<DataBase> territories = SimulationManagement.GetDataViaTag(DataTags.Territory);

					//Draw per territory data
					foreach (TerritoryData territoryData in territories.Cast<TerritoryData>())
					{
						int id = territoryData.parent.Get().id;
						bool hasEmblemData = entityIDtoEmblemDatas.ContainsKey(id);

                        int count = territoryData.borders.Count;

						//Traverse along the border of the territory
						//Find all continous edges
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
								Color color = Color.white;
								if (hasEmblemData)
								{
									color = entityIDtoEmblemDatas[id].mainColour;
								}

#pragma warning disable CS0618 // Type or member is obsolete
                                lineRenderer.SetColors(color, color);
#pragma warning restore CS0618 // Type or member is obsolete

                                lineRenderer.positionCount = line.Count;
                                lineRenderer.SetPositions(line.ToArray());

                                lineRenderer.loop = true;
                            }
                        }

                        if (hasEmblemData)
                        {
                            Transform centralIcon = mapElementsPools.UpdateNextObjectPosition(5, iconPos - (Vector3.up * 0.25f));
                            centralIcon.localScale = iconScale;

                            if (!cachedTransformToNationIconRenderers.ContainsKey(centralIcon))
                            {
                                cachedTransformToNationIconRenderers.Add(centralIcon, centralIcon.GetComponent<SpriteRenderer>());
                            }

                            cachedTransformToNationIconRenderers[centralIcon].sprite = entityIDtoEmblemDatas[id].mainIcon;
                            cachedTransformToNationIconRenderers[centralIcon].color = entityIDtoEmblemDatas[id].highlightColour;
                        }
                    }

                    //Get all military data for marked transfer drawing
                    PathHelper.SimplePathParameters pathParams = new PathHelper.SimplePathParameters();
                    List<DataBase> militaryDatas = SimulationManagement.GetDataViaTag(DataTags.Military);

					foreach (MilitaryData militaryData in militaryDatas)
					{
                        int id = militaryData.parent.Get().id;
                        bool hasEmblemData = entityIDtoEmblemDatas.ContainsKey(id);

                        Color color = Color.white;
                        if (hasEmblemData)
                        {
                            color = entityIDtoEmblemDatas[id].mainColour;
                        }


                        foreach ((RealSpacePostion, RealSpacePostion) entry in militaryData.markedTransfers)
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
                                lineRenderer.SetColors(color, color);
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


