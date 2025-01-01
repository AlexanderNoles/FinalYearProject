using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using EntityAndDataDescriptor;
using static UnityEngine.EventSystems.EventTrigger;
using System.Linq;

public class HistoryUIManagement : UIState
{
	public static HistoryUIManagement instance;
	public MultiObjectPool pixelPool;
	public GameObject mainUI;
	public Image historyBar;
	private float lastPercentage = -1.0f;
	private const float minimumChange = 0.01f;
	public TextMeshProUGUI label;

	private Dictionary<Transform, Image> transToImage = new Dictionary<Transform, Image>();

    protected override GameObject GetTargetObject()
    {
        return mainUI;
    }

    protected override void OnSetActive(bool _bool)
    {
		if (_bool)
		{
			lastPercentage = -1.0f;
		}
    }

    public static void SetHistoryUIActive()
    {
		if (instance != null)
		{
			UIManagement.LoadUIState(instance);
		}
    }

    protected override void Awake()
    {
        instance = this;
        base.Awake();
    }

    private void Update()
	{
		if (mainUI.activeSelf)
		{
			historyBar.fillAmount = SimulationManagement.GetHistoryRunPercentage();

			if (historyBar.fillAmount >= 1.0f)
			{
				UIManagement.ReturnToNeutral();
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

		//Get targets
		List<DataBase> targets = SimulationManagement.GetDataViaTag(DataTags.Territory);
		//Get their emblem data
		List<EmblemData> correspondingEmblemData = SimulationManagement.TryGetDataIntoClone<EmblemData>(DataTags.Emblem, targets);

		//Clone targets into a usable list
		List<TerritoryData> clonedTargets = targets.Cast<TerritoryData>().ToList();

		GameWorld.main.GetData(DataTags.Historical, out HistoryData historyData);
		Color historyColour = Color.white * 0.3f;
		
		double gridDensity = WorldManagement.GetGridDensity();
		int range = (int)Math.Round(WorldManagement.GetSolarSystemRadius() / gridDensity);
		for (int x = -range; x <= range; x++)
		{
			for (int z = -range; z <= range; z++)
			{
				RealSpacePostion currentCell = WorldManagement.ClampPositionToGrid(new RealSpacePostion(x * gridDensity, 0, z * gridDensity));

				try
				{
                    if (historyData != null && historyData.previouslyOwnedTerritories.ContainsKey(currentCell))
                    {
                        AddPixel(currentCell, historyColour);
                        continue;
                    }
                }
				catch (NullReferenceException)
				{
					continue;
				}

                for (int i = 0; i < clonedTargets.Count; i++)
				{
                    if (clonedTargets[i].territoryCenters.Contains(currentCell))
                    {
                        AddPixel(currentCell, correspondingEmblemData[i].mainColour);
                        break;
                    }
                }
			}
		}

		pixelPool.PruneObjectsNotUpdatedThisFrame(0);
	}

	private void AddPixel(RealSpacePostion postion, Color color)
	{
		Transform pixelTrans = pixelPool.UpdateNextObjectPosition(0, Vector3.zero);

		(pixelTrans as RectTransform).anchoredPosition = postion.AsTruncatedVector2(600.0f);

		if (!transToImage.ContainsKey(pixelTrans))
		{
			transToImage.Add(pixelTrans, pixelTrans.GetComponent<Image>());
		}

		transToImage[pixelTrans].color = color;
	}
}
