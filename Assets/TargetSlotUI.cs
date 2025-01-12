using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TargetSlotUI : SlotUI
{
	[HideInInspector]
	public new GameObject gameObject;

	public TextMeshProUGUI targetNameLabel;

	public void SetActive(bool active)
	{
		gameObject.SetActive(active);
	}

	private void Awake()
	{
		gameObject = base.gameObject;
	}

	public void DrawTargetSpecific(BattleBehaviour target)
	{
		targetNameLabel.text = target.targetsName;
	}
}
