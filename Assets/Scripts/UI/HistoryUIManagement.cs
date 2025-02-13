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
	public RectTransform historyBarEmpty;
	public GameObject historyBarOriginal;
	private Image barTarget = null;
	private float barOffset = 0.0f;
	private HistoryData historyTarget = null;
	private int cachedPeriodID;
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

	public static void SetHistoryUIInactive()
	{
		if (instance != null)
		{
			instance.SetActive(false);
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

    private void Start()
    {
		GameWorld.main.GetData(DataTags.Historical, out historyTarget);
		cachedPeriodID = historyTarget.GetCurrentPeriod().periodID;

		CreateNewHistoryBar(historyTarget.GetCurrentPeriod().color);
    }

    private void Update()
	{
		if (mainUI.activeSelf)
		{
			float value = SimulationManagement.GetHistoryRunPercentage();

            UpdateProgressBar(value);

			if (Mathf.Abs(value - lastPercentage) > minimumChange)
			{
				lastPercentage = value;

				label.text = $"Running History...\n({SimulationManagement.GetDateString()})";

				UpdateTerritoryMap();
			}
		}
	}

	private const float barSeperator = 0.005f;

	private void CreateNewHistoryBar(Color color)
	{
		bool hidPreviousBar = false;
		if (barTarget != null)
		{
			float cachedFillAmount = barTarget.fillAmount;

			//Offset old bar back slightly to create a little border between bars
            barTarget.fillAmount -= barSeperator;
			barTarget.fillAmount = Mathf.Clamp01(barTarget.fillAmount);

			hidPreviousBar = barTarget.fillAmount <= 0.0035f;

			if (!hidPreviousBar)
            {
                //Cache offset so bar starts from new point
                barOffset = cachedFillAmount + barOffset;
            }
        }

		//If we zerod the previous bar it is now removed and we can just reuse that one
		//This means that some periods won't be represetned (but they took up so little time they really shouldn't be)
		if (!hidPreviousBar)
		{
            //Get new bar target
            barTarget = Instantiate(historyBarOriginal, historyBarEmpty).GetComponent<Image>();

            //Rotate bar so if starts from new point
            barTarget.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -barOffset * 360.0f);
        }

		//Set new bar color
		barTarget.color = color;
	}

	private void UpdateProgressBar(float value)
	{
		if (value < 1.0f)
        {
			if (historyTarget.GetCurrentPeriod().periodID != cachedPeriodID)
			{
                HistoryData.Period newTarget = historyTarget.GetCurrentPeriod();
                cachedPeriodID = newTarget.periodID;

                CreateNewHistoryBar(newTarget.color);
            }

            barTarget.fillAmount = Mathf.Clamp(value - barOffset, 0.0f, 1.0f - barSeperator);
        }
    }

	private void UpdateTerritoryMap()
	{
		//Get all current territories
		//Iterate over every position on map
		//Update map pixels to match

		//Get targets
		List<DataModule> targets = SimulationManagement.GetDataViaTag(DataTags.Territory);
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
				RealSpacePosition currentCell = WorldManagement.ClampPositionToGrid(new RealSpacePosition(x * gridDensity, 0, z * gridDensity));

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

	private void AddPixel(RealSpacePosition postion, Color color)
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
