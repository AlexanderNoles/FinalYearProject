using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class NationInformationDisplayUI : MonoBehaviour
{
	private static Dictionary<int, int> entityIDtoResourceCount = new Dictionary<int, int>();
	private static NationInformationDisplayUI instance;
	private static SimulationEntity simulationEntity;
	public GameObject target;
	public EmblemRenderer emblemRenderer;
	public TextMeshProUGUI title;
	public TextMeshProUGUI founded;
	public RectTransform indicator;
	private Vector3 startingIndicatorPos;
	private Vector3 targetIndicatorPos;
	private float onDrawAnimT = 1.0f;
	public TextMeshProUGUI populationLabel;
	public TextMeshProUGUI resourceNodeLabel;

	[Header("Animation Control")]
	public AnimationCurve politicalIndicatorAnimationCurve;
	public float politicalIndicatorAnimSpeed = 3.0f;

	private void Awake()
	{
		instance = this;
		entityIDtoResourceCount.Clear();

		//Set target to position, this is done because during testing it is typically elsewhere
		//so it can be adjusted outside of the context of other ui elements
		(target.transform as RectTransform).anchoredPosition = new Vector3(250, -375, 0.0f);

		SetActive(false, null);
	}

	public static bool IsActive()
	{
		if (instance == null)
		{
			return false;
		}

		return instance.target.activeSelf;
	}

	public static bool IsTarget(SimulationEntity entity)
	{
		return entity.Equals(simulationEntity);
	}

	public static void SetActive(bool _bool, SimulationEntity entity)
	{
		instance.target.SetActive(_bool);

		if (_bool)
		{
			simulationEntity = entity;
			instance.Draw(entity);
		}
		else
		{
			simulationEntity = null;
		}
	}

	private void Draw(SimulationEntity entity)
	{
		//Generate lookup data for mineral deposits
		if (entityIDtoResourceCount.Count == 0)
		{
			//Get all territories
			List<TerritoryData> territories = SimulationManagement.GetDataViaTag(DataTags.Territory).Cast<TerritoryData>().ToList();

			foreach (TerritoryData terr in territories)
			{
				int count = 0;
				foreach (RealSpacePosition pos in terr.territoryCenters)
				{
					if (TargetableLocationData.targetableLocationLookup.ContainsKey(pos))
					{
						count++;
					}
				}

				entityIDtoResourceCount[terr.parent.Get().id] = count;
			}
		}
		//

		//Get emblem data
		if (entity.GetData(DataTags.Emblem, out EmblemData emblemData))
		{
			emblemRenderer.Draw(emblemData);
		}

		founded.text = $"// Founded {entity.createdYear}U //";

		Nation nation = entity as Nation;
		title.text = nation.GetDataDirect<NameData>(DataTags.Name).GetName();

		if (entity.GetData(DataTags.Political, out PoliticalData politicalData))
		{
			const float boundsMinMax = 87.5f;
			startingIndicatorPos = indicator.anchoredPosition3D;
			targetIndicatorPos = new Vector3(politicalData.authorityAxis * boundsMinMax, politicalData.economicAxis * boundsMinMax, 0.0f);
		}

		if (entity.GetData(DataTags.Population, out PopulationData populationData))
		{
			populationLabel.text = $"Population Count: {Mathf.RoundToInt(populationData.currentPopulationCount)/100.0f}M";
		}

		if (entityIDtoResourceCount.ContainsKey(entity.id))
		{
			resourceNodeLabel.text = $"Resource Nodes: {entityIDtoResourceCount[entity.id]}";
		}

		onDrawAnimT = 0.0f;
	}

	private void Update()
	{
		if (onDrawAnimT < 1.0f)
		{
			onDrawAnimT += Time.deltaTime;

			indicator.anchoredPosition3D = Vector3.Lerp(startingIndicatorPos, targetIndicatorPos, politicalIndicatorAnimationCurve.Evaluate(onDrawAnimT * politicalIndicatorAnimSpeed));
		}
	}

	public void SelectButtonCallback()
	{
		NationSelectionInteraction.FinalizeNationSelect(simulationEntity);
	}

	public void CloseUIButtonCallback()
	{
		SetActive(false, null);
	}

	public void ReloadButtonCallback()
	{
		GameManagement.ReloadScene();
	}
}
