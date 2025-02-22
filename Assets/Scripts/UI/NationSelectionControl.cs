using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class NationSelectionControl : MonoBehaviour
{
	public TextMeshProUGUI subtitle;

	private static NationSelectionControl instance;
	public EntityInformationDisplay target;

	private void Awake()
	{
		instance = this;
		
		SetActive(false, null);
	}

	private void OnEnable()
	{
		subtitle.text = $"// Year: {SimulationManagement.GetCurrentYear()}U //";
	}

	public static bool IsTarget(SimulationEntity entity)
	{
		return entity.Equals(instance.target.GetTarget());
	}

	public static void SetActive(bool _bool, SimulationEntity entity)
	{
		instance.target.SetActive(_bool, entity);
	}

	public void SelectButtonCallback()
	{
		NationSelectionInteraction.FinalizeNationSelect(target.GetTarget());
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
