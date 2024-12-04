using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HistoryUIManagement : MonoBehaviour
{
	public GameObject mainUI;
	public Image historyBar;
	private float lastPercentage = -1.0f;
	private const float minimumChange = 0.01f;
	public TextMeshProUGUI label;

	public void Activate()
	{
		mainUI.SetActive(true);
		lastPercentage = -1.0f;
	}

	public void Deactivate()
	{
		mainUI.SetActive(false);
	}

	private void Update()
	{
		if (mainUI.activeSelf)
		{
			historyBar.fillAmount = SimulationManagement.GetHistoryRunPercentage();

			if (historyBar.fillAmount >= 1.0f)
			{
				Deactivate();
			}

			if (Mathf.Abs(historyBar.fillAmount - lastPercentage) > minimumChange)
			{
				lastPercentage = historyBar.fillAmount;

				label.text = $"Running History...\n({SimulationManagement.GetDateString()})";
			}
		}
	}
}
