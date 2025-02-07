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

	public class BorderRender
	{
		public List<LineRenderer> targets = new List<LineRenderer>();
		public Color baseColor;
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
    private const int internalTerritoryIndicatorPool = 10;

	private float mapRefreshTime;
    private const bool autoUpdateMap = true;

	private List<SurroundingObject> mapTargetObjects;
    private Dictionary<Transform, MeshRenderer> mapRingMeshRenderes;
	private Dictionary<Transform, LineRenderer> cachedTransformToBorderRenderer = new Dictionary<Transform, LineRenderer>();
	private Dictionary<Transform, SpriteRenderer> cachedTransformToNationIconRenderers = new Dictionary<Transform, SpriteRenderer>();
	private Dictionary<Transform, Material> cachedTransformToMaterial = new Dictionary<Transform, Material>();
	private List<TroopTransferEffect> ttEffects = new List<TroopTransferEffect>();
	private static Dictionary<int, BorderRender> idToBorderRender = new Dictionary<int, BorderRender>();
	private static Dictionary<int, Color> borderColourOverridesLastFrame = new Dictionary<int, Color>();
	private static Dictionary<int, Color> borderColourOverridesThisFrame = new Dictionary<int, Color>();

	public static void CreateBorderColourOverride(int id, Color color)
	{
		if (!borderColourOverridesThisFrame.ContainsKey(id))
		{
			borderColourOverridesThisFrame.Add(id, color);
		}
		else
		{
			borderColourOverridesThisFrame[id] = color;
		}
	}


	public AnimationCurve troopTransferCurve;

    protected override void Awake()
    {
		instance = this;
        base.Awake();
    }

    private void Start()
    {
        mapRingMeshRenderes = mapElementsPools.GetComponentsOnAllActiveObjects<MeshRenderer>(0);

		borderColourOverridesThisFrame.Clear();
		borderColourOverridesLastFrame.Clear();
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
					target.transform.localPosition = -target.position.AsTruncatedVector3(mapRelativeScaleModifier) + currentOffset;

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
					List<DataModule> territories = SimulationManagement.GetDataViaTag(DataTags.Territory);

					//Reset lookupt table
					idToBorderRender.Clear();

					Vector3 additionalTerrOffset = Vector3.down * 0.1f;
					//Draw per territory data
					foreach (TerritoryData territoryData in territories.Cast<TerritoryData>())
					{
						int id = territoryData.parent.Get().id;
						bool hasEmblemData = entityIDtoEmblemDatas.ContainsKey(id);
						Color color = Color.white;
						if (hasEmblemData)
						{
							color = entityIDtoEmblemDatas[id].mainColour;
						}

						//Traverse along the border of the territory
						//Find all continous edges
						List<List<Vector3>> borderLines = territoryData.CalculateMapBorderPositions(out Vector3 iconPos, out Vector3 iconScale, shiftModifier);

						foreach (List<Vector3> line in borderLines)
						{
							Transform borderRendererTransform = mapElementsPools.UpdateNextObjectPosition(3, additionalTerrOffset);

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

								lineRenderer.positionCount = line.Count;
								lineRenderer.SetPositions(line.ToArray());

								lineRenderer.loop = true;
							}

							//Add line renderer to lookup table so it can be used for effects
							if (!idToBorderRender.ContainsKey(id))
							{
								idToBorderRender.Add(id, new BorderRender());
								idToBorderRender[id].baseColor = color;
							}

							idToBorderRender[id].targets.Add(lineRenderer);
						}

						if (hasEmblemData)
						{
							//If this entity has settlements set iconPos to be placed ontop of their oldest one instead
							if (territoryData.TryGetLinkedData(DataTags.Settlements, out SettlementsData setData))
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

						//Draw internal squares
						foreach (RealSpacePosition pos in territoryData.territoryCenters)
						{
							Transform newTrans = mapElementsPools.UpdateNextObjectPosition(internalTerritoryIndicatorPool, additionalTerrOffset - pos.AsTruncatedVector3(mapRelativeScaleModifier));

							if (!cachedTransformToMaterial.ContainsKey(newTrans))
							{
								cachedTransformToMaterial.Add(newTrans, newTrans.GetComponent<MeshRenderer>().material);
							}

							cachedTransformToMaterial[newTrans].SetColor("_Color", color);
						}
					}

                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(3);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(5);
                    mapElementsPools.PruneObjectsNotUpdatedThisFrame(6);
					mapElementsPools.PruneObjectsNotUpdatedThisFrame(internalTerritoryIndicatorPool);
				}

				//Update troop transfer effects
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

				//Border colour overrides
				//Each frame apply the colour overrides
				//If exists in last frames, remove it from last frames
				//Should be left with all the last frames not updated this frame
				//Replace last frame with this frames
				//Repeat
				foreach (KeyValuePair<int, Color> entry in borderColourOverridesThisFrame)
				{
					int id = entry.Key;
					if (idToBorderRender.ContainsKey(id))
					{
						if (borderColourOverridesLastFrame.ContainsKey(id))
						{
							borderColourOverridesLastFrame.Remove(id);
						}

						foreach (LineRenderer lr in idToBorderRender[id].targets)
						{
#pragma warning disable CS0618 // Type or member is obsolete
							lr.SetColors(entry.Value, entry.Value);
#pragma warning restore CS0618 // Type or member is obsolete
						}
					}
				}

				//Any entry left in last frame was updated last frame but not this one, meaning the override has dissappeard
				foreach (KeyValuePair<int, Color> entry in borderColourOverridesLastFrame)
				{
					int id = entry.Key;
					if (idToBorderRender.ContainsKey(id))
					{
						Color baseColour = idToBorderRender[id].baseColor;
						foreach (LineRenderer lr in idToBorderRender[id].targets)
						{
#pragma warning disable CS0618 // Type or member is obsolete
							lr.SetColors(baseColour, baseColour);
#pragma warning restore CS0618 // Type or member is obsolete
						}
					}
				}

				//Overwrite
				borderColourOverridesLastFrame = borderColourOverridesThisFrame;
				//Clear
				borderColourOverridesThisFrame = new Dictionary<int, Color>();
            }
        }
    }
}


