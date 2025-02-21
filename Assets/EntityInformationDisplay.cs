using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EntityInformationDisplay : StateOverride
{
	[Header("External Interaction")]
	public bool setAsInstance = false;
	private static EntityInformationDisplay instance;

	private SimulationEntity currentTarget;

	public SimulationEntity GetTarget()
	{
		return currentTarget;
	}

	[Header("References")]
	public EmblemRenderer emblemRenderer;
	public TextMeshProUGUI title;
	public TextMeshProUGUI foundedLabel;
	public RectTransform politicsIndicator;
	private Vector3 startingIndicatorPos;
	private Vector3 endIndicatorPos;
	private float onDrawAnimT = 1.0f;
	public TextMeshProUGUI populationLabel;
	public TextMeshProUGUI resourceNodeLabel;

	[Header("Animation Control")]
	public AnimationCurve polIndicatorAnimationCurve;
	public float politicalIndicatorAnimSpeed = 3.0f;

	[Header("On Active Control")]
	public bool predeterminedPosition = false;
	public Vector3 position = new Vector3(250, -375, 0.0f);
	public DraggableWindow draggableWindow;

	private void Awake()
	{
		if (!setAsInstance)
		{
			return;
		}

		instance = this;
		SetActiveExternal(false, null);
	}

	public static void ToggleExternal(SimulationEntity target)
	{
		bool setActive = !instance.gameObject.activeSelf;

		if (!setActive)
		{
			target = null;
		}

		SetActiveExternal(setActive, target);
	}

	public static void SetActiveExternal(bool active, SimulationEntity target)
	{
		instance.SetActive(active, target);
	}

	public void SetActive(bool active, SimulationEntity target)
	{
		gameObject.SetActive(active);

		if (active)
		{
			currentTarget = target;
			Draw(currentTarget);

			if (predeterminedPosition)
			{
				(transform as RectTransform).anchoredPosition = position;
			}
			else
			{
				draggableWindow.InitialOffset();
			}
		}
		else
		{
			currentTarget = null;
		}
	}

	private void Draw(SimulationEntity entity)
	{
		int resourceNodeCount = 0;

		//Calculate the amount of resource nodes
		//Iterate through all territory nodes and find if they have a position there
		if (entity.GetData(DataTags.Territory, out TerritoryData territoryData))
		{
			foreach (RealSpacePosition pos in territoryData.territoryCenters)
			{
				if (TargetableLocationData.targetableLocationLookup.ContainsKey(pos))
				{
					resourceNodeCount += TargetableLocationData.targetableLocationLookup[pos].Count;
				}
			}
		}
		//

		//Retrieve emblem data
		if (entity.GetData(DataTags.Emblem, out EmblemData emblemData))
		{
			emblemRenderer.Draw(emblemData);
		}

		title.text = entity.GetDataDirect<NameData>(DataTags.Name).GetName();
		foundedLabel.text = $"// Founded {entity.createdYear}U //";

		if (entity.GetData(DataTags.Political, out PoliticalData politicalData))
		{
			const float boundsMinMax = 87.5f;
			startingIndicatorPos = politicsIndicator.anchoredPosition3D;
			endIndicatorPos = new Vector3(politicalData.authorityAxis * boundsMinMax, politicalData.economicAxis * boundsMinMax, 0.0f);
		}

		if (entity.GetData(DataTags.Population, out PopulationData populationData))
		{
			populationLabel.text = $"Population Count: {Mathf.RoundToInt(populationData.currentPopulationCount) / 100.0f}M";
		}

		resourceNodeLabel.text = $"Resource Nodes: {resourceNodeCount}";

		//Reset on draw anim
		//Goes from 0 to 1, so we can multiply it by a value to speed up an anim locally
		onDrawAnimT = 0.0f;
	}

	private void Update()
	{
		if (onDrawAnimT < 1.0f)
		{
			onDrawAnimT += Time.deltaTime;

			politicsIndicator.anchoredPosition3D = Vector3.Lerp(startingIndicatorPos, endIndicatorPos, polIndicatorAnimationCurve.Evaluate(onDrawAnimT * politicalIndicatorAnimSpeed));
		}
	}

	public void CloseUICallback()
	{
		SetActive(false, null);
	}
}
