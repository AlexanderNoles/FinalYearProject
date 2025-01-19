using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using EntityAndDataDescriptor;
using static UnityEngine.EventSystems.EventTrigger;

public class MapManagement : UIState
{
	/// UI STATE IMPLEMENTATION
	private static MapManagement instance;
    public const float mapRelativeScaleModifier = 1000.0f;
	public float shiftModifier;
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

		if (autoSetupDone)
		{
			mapElementsPools.gameObject.SetActive(_bool);
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

	protected override GameObject GetTargetObject()
	{
		return target;
	}

	public GameObject target;
	[Header("Map Settings")]
	public FadeOnEnable fadeInEffect;
    public MultiObjectPool mapElementsPools;
    private const int mapRingPool = 0;
    private const int shipIndicatorPool = 1;
    private const int mapBasePool = 2;

    private float mapRefreshTime;
    private const bool autoUpdateMap = true;

	private List<SurroundingObject> mapTargetObjects;
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
			Vector3 playerPos = -PlayerCapitalShip.GetPCSPosition().AsTruncatedVector3(mapRelativeScaleModifier);
			mapElementsPools.UpdateNextObjectPosition(shipIndicatorPool, playerPos);
			mapElementsPools.PruneObjectsNotUpdatedThisFrame(shipIndicatorPool);

			if (MapIntroRunning())
            {
				if (FirstFrameMapIntroRunning())
				{
					mapTargetObjects = SurroundingsRenderingManagement.GetControlledObjects();

					foreach (SurroundingObject target in mapTargetObjects)
					{
						//Indicate we are not using the shell system
						target.SetShellOffset(-1);
						//Set on map scale
						target.SetObjectVisualScale(target.scale / mapRelativeScaleModifier);
					}

					//Ensure controlled objects are shown
					SurroundingsRenderingManagement.SetAllSurroundingsActive(true);

					//Disable skybox
					SurroundingsRenderingManagement.SetSkyboxActive(false);

					//Undraw all border rendering lines
					mapElementsPools.PruneObjectsNotUpdatedThisFrame(3, true);
					mapElementsPools.PruneObjectsNotUpdatedThisFrame(5, true);

					//Hide journey and destination indicator
					mapElementsPools.HideAllObjects(7);
					mapElementsPools.HideAllObjects(8);
				}
				else if (LastFrameOfMapIntro())
				{
					Shader.SetGlobalFloat("_FlashTime", Time.time);
					mapRefreshTime = 0.0f;
				}

				float evaluatedIntroT = EvaluatedMapIntroT();
				Vector3 currentOffset = Vector3.down * Mathf.Lerp(0.0f, CameraManagement.cameraOffsetInMap, 1.0f - evaluatedIntroT);
				foreach (SurroundingObject target in mapTargetObjects)
				{
					//Update surrounding objects
					target.transform.localPosition = -target.postion.AsTruncatedVector3(mapRelativeScaleModifier) + currentOffset;

					//Update UI
					if (target.GetInWorldParent() != null)
					{
						Vector3 orbitIndicatorPos = target.GetInWorldParent().position;
						//Set orbit indicator to target height
						orbitIndicatorPos.y = target.transform.position.y;
						//Calculate scale
						Vector3 scale = target.transform.position - orbitIndicatorPos;
						scale.y = 0;

						float scaleMag = Mathf.Max(1, scale.magnitude);
						Transform orbitIndicatorRing = mapElementsPools.UpdateNextObjectPosition(mapRingPool, orbitIndicatorPos);

						orbitIndicatorRing.localScale = new Vector3(scaleMag, 1, scaleMag) * 2;

						mapRingMeshRenderes[orbitIndicatorRing].material.SetFloat("_Radius", scaleMag);
						mapRingMeshRenderes[orbitIndicatorRing].material.SetVector("_RingItemPos", target.transform.position);
						mapRingMeshRenderes[orbitIndicatorRing].material.SetFloat("_RingItemRadius", target.transform.localScale.magnitude * 0.75f);

						if (FirstFrameMapIntroRunning())
						{
							mapRingMeshRenderes[orbitIndicatorRing].material.SetFloat("_Thickness", Mathf.Max(0.1f, 0.3f * target.transform.localScale.x));
						}
					}
				}

				mapElementsPools.UpdateNextObjectPosition(mapBasePool, Vector3.up * Mathf.Lerp(-25, -0.1f, EvaluatedMapIntroT()));

				mapElementsPools.PruneObjectsNotUpdatedThisFrame(mapRingPool);
				mapElementsPools.PruneObjectsNotUpdatedThisFrame(mapBasePool);
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

						//Traverse along the border of the territory
						//Find all continous edges
						List<List<Vector3>> borderLines = territoryData.CalculateMapBorderPositions(out Vector3 iconPos, out Vector3 iconScale, shiftModifier);

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
							//If this entity has settlements set iconPos to be placed ontop of their oldest one instead
							if (territoryData.TryGetLinkedData(DataTags.Settlement, out SettlementData setData))
							{
								if (setData.settlements.Count > 0)
								{
									iconPos = -setData.settlements.First().Value.actualSettlementPos.AsTruncatedVector3(mapRelativeScaleModifier);
								}
							}

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
//                    PathHelper.SimplePathParameters pathParams = new PathHelper.SimplePathParameters();
//                    List<DataBase> militaryDatas = SimulationManagement.GetDataViaTag(DataTags.Military);

//					foreach (MilitaryData militaryData in militaryDatas)
//					{
//                        int id = militaryData.parent.Get().id;
//                        bool hasEmblemData = entityIDtoEmblemDatas.ContainsKey(id);

//                        Color color = Color.white;
//                        if (hasEmblemData)
//                        {
//                            color = entityIDtoEmblemDatas[id].mainColour;
//                        }

//                        foreach ((RealSpacePostion, RealSpacePostion) entry in militaryData.markedTransfers)
//                        {
//                            Transform borderRendererTransform = mapElementsPools.UpdateNextObjectPosition(3, Vector3.zero);

//                            if (!cachedTransformToBorderRenderer.ContainsKey(borderRendererTransform))
//                            {
//                                cachedTransformToBorderRenderer.Add(borderRendererTransform, borderRendererTransform.GetComponent<LineRenderer>());
//                            }

//                            LineRenderer lineRenderer = cachedTransformToBorderRenderer[borderRendererTransform];

//                            if (lineRenderer != null)
//                            {
//#pragma warning disable CS0618 // Type or member is obsolete
//                                lineRenderer.SetColors(color, color);
//#pragma warning restore CS0618 // Type or member is obsolete

//                                Vector3 startPos = -entry.Item1.AsTruncatedVector3(mapRelativeScaleModifier);
//                                RealSpacePostion endPosRS;

//                                //Draw to actual battle pos
//                                if (globalBattleData.battles.ContainsKey(entry.Item2))
//                                {
//                                    endPosRS = globalBattleData.battles[entry.Item2].GetPosition();
//                                }
//                                else
//                                {
//                                    endPosRS = entry.Item2;
//                                }

//                                Vector3 endPos = -endPosRS.AsTruncatedVector3(mapRelativeScaleModifier);

//                                Vector3 difference = endPos - startPos;
//                                pathParams.forwardVector = difference.normalized;
//                                pathParams.rightVector = Vector3.up * (difference.magnitude * 0.4f);

//                                PathHelper.SimplePath path = PathHelper.GenerateSimplePathStatic(startPos, endPos, pathParams);
//                                int res = Mathf.CeilToInt(path.EstimateLength() / 2.5f);
//                                res = Mathf.Max(4, res);

//                                lineRenderer.loop = false;
//                                lineRenderer.positionCount = res;

//                                TroopTransferEffect newTTE = new TroopTransferEffect
//                                {
//                                    length = timeTillNextMapUpdate * Random.Range(0.1f, 0.2f),
//                                    path = path,
//                                    pathResolution = res,
//                                    startTime = Time.time,
//                                    target = lineRenderer
//                                };
//                                ttEffects.Add(newTTE);
//                            }
//                        }
//                    }

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


