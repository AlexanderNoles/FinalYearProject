using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NationInformationDisplayUI : MonoBehaviour
{
	private static NationInformationDisplayUI instance;
	private static SimulationEntity simulationEntity;
	public GameObject target;
	public EmblemRenderer emblemRenderer;
	public TextMeshProUGUI title;
	public TextMeshProUGUI founded;

	private void Awake()
	{
		instance = this;
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
		//Get emblem data
		if (entity.GetData(DataTags.Emblem, out EmblemData emblemData))
		{
			emblemRenderer.Draw(emblemData);
		}

		founded.text = $"// Founded {entity.createdYear}U //";

		Nation nation = entity as Nation;
		title.text = nation.name;
	}

	public void SelectButtonCallback()
	{
		NationSelectionInteraction.FinalizeNationSelect(simulationEntity);
	}
}
