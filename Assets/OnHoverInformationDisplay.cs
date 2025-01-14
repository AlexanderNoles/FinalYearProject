using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OnHoverInformationDisplay : MonoBehaviour
{
	private static OnHoverInformationDisplay instance;
	private IDisplay currentTarget;
	private GameObject hoverSource;

	public GameObject mainUI;
	public TextMeshProUGUI titleLabel;
	public TextMeshProUGUI descLabel;
	public TextMeshProUGUI extraLabel;

	private void Awake()
	{
		instance = this;
		Hide();
	}

	public static void SetCurrentTarget(IDisplay newTarget, GameObject hoverSource)
	{
		instance.currentTarget = newTarget;
		instance.hoverSource = hoverSource;
		instance.Draw();
	}

	public static void RemoveCurrentTarget(IDisplay oldTarget)
	{
		if (instance.currentTarget.Equals(oldTarget))
		{
			//Currently displaying this 
			instance.Hide();
		}
	}

	private void Draw()
	{
		mainUI.SetActive(true);

		titleLabel.text = currentTarget.GetTitle();
		descLabel.text = currentTarget.GetDescription();
		extraLabel.text = currentTarget.GetExtraInformation();
	}

	private void Hide()
	{
		mainUI.SetActive(false);
	}

	private void Update()
	{
		if (mainUI.activeSelf && (hoverSource != null && !hoverSource.activeInHierarchy))
		{
			//This occurs when the object being hovered over is set inactive
			Hide();
		}
	}
}
