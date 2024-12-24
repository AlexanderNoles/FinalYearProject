using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HistoryUIManagement : MonoBehaviour
{
	public MultiObjectPool pixelPool;
	public GameObject mainUI;
	public Image historyBar;
	private float lastPercentage = -1.0f;
	private const float minimumChange = 0.01f;
	public TextMeshProUGUI label;

	private Dictionary<Transform, Image> transToImage = new Dictionary<Transform, Image>();

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

				UpdateTerritoryMap();
			}
		}
	}

	private void UpdateTerritoryMap()
	{
		//Get all current territories
		//Iterate over every position on map
		//Update map pixels to match
		List<Faction> factions = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Territory);
		List<(TerritoryData, Color)> territoryDatas = new List<(TerritoryData, Color)>();

		int factionDataNotGrabbed = factions.Count;

		foreach (Faction faction in factions)
		{
			if (faction.GetData(Faction.Tags.Territory, out TerritoryData newTerrData))
			{
				if (faction.GetData(Faction.Tags.Emblem, out EmblemData emblemData))
				{
					territoryDatas.Add((newTerrData, emblemData.mainColour));
				}
			}

			factionDataNotGrabbed--;
		}

		//This happens if the above for loop fails because some outside force (the multithreaded history simulation)
		//Effects the underlying data
		if (factionDataNotGrabbed != 0)
		{
			return;
		}
		
		double gridDensity = WorldManagement.GetGridDensity();
		int range = (int)Math.Round(WorldManagement.GetSolarSystemRadius() / gridDensity);
		for (int x = -range; x <= range; x++)
		{
			for (int z = -range; z <= range; z++)
			{
				RealSpacePostion currentCell = WorldManagement.ClampPositionToGrid(new RealSpacePostion(x * gridDensity, 0, z * gridDensity));

				foreach ((TerritoryData, Color) entry in territoryDatas)
				{
					if (entry.Item1.territoryCenters.Contains(currentCell))
					{
						Transform pixelTrans = pixelPool.UpdateNextObjectPosition(0, Vector3.zero);
						(pixelTrans as RectTransform).anchoredPosition = currentCell.AsTruncatedVector2(600.0f);

						if (!transToImage.ContainsKey(pixelTrans))
						{
							transToImage.Add(pixelTrans, pixelTrans.GetComponent<Image>());
						}

						transToImage[pixelTrans].color = entry.Item2;

						continue;
					}
				}
			}
		}


		pixelPool.PruneObjectsNotUpdatedThisFrame(0);
	}
}
