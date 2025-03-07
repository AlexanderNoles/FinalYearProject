using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UIHelper
{
	public static string ConvertToNiceNumberString(float input, int numberOfDecimalPlaces = 2)
	{
		if (input >= 1000000)
		{
			return System.Math.Round(input / 1000000, numberOfDecimalPlaces).ToString() + "M";
		}
		else if (input >= 1000)
		{
			return System.Math.Round(input / 1000, numberOfDecimalPlaces).ToString() + "K";
		}

		return Mathf.RoundToInt(input).ToString();
	}

	public static List<RaycastResult> ElementsUnderMouse()
	{
		PointerEventData pointerData = new PointerEventData(EventSystem.current)
		{
			pointerId = -1,
		};

		pointerData.position = Input.mousePosition;

		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerData, results);

		return results;
	}

	public static List<Vector3> CalculateRowedPositions(int count, Vector3 initialOffset, Vector3 offsetPerElement, Vector3 offsetPerRow, int countPerRow = 4)
	{
		List<Vector3> toReturn = new List<Vector3>();

		int currentCount = 0;
		int currentRow = 0;

		for (int i = 0; i < count; i++)
		{
			Vector3 calculatedPos = initialOffset;

			calculatedPos += offsetPerElement * currentCount;
			calculatedPos += offsetPerRow * currentRow;

			toReturn.Add(calculatedPos);

			currentCount++;

			if (currentCount >= countPerRow)
			{
				currentCount = 0;
				currentRow++;
			}
		}

		return toReturn;
	}

	public static List<Vector3> CalculateSpreadPositions(int count, int bufferBetween, int flipThreshold = 6)
	{
		List<Vector3> toReturn = new List<Vector3>();
		List<float> prios = new List<float>();

		//!Here we should really be finding the pair of factors that have a minimum distance from each other
		//So we don't end up with shapes that are stretched looking in one axis

		//Establish the two highest factors
		List<int> factors = new List<int>();
		int visualCount = count - (count % 2);

		int max = Mathf.FloorToInt(Mathf.Sqrt(visualCount));

		for (int potentialFactor = max; potentialFactor >= 1 && factors.Count < 2; potentialFactor--)
		{
			if (visualCount % potentialFactor == 0) //Is a factor
			{
				factors.Add(potentialFactor);

				//Don't add the square root twice
				if (potentialFactor != visualCount / potentialFactor)
				{
					factors.Add(visualCount / potentialFactor);
				}
			}
		}

		if (factors.Count == 0)
		{
			//This indicates a count of <= 0
			return toReturn;
		}

		//Find max factor for y and min factor for x, this is done instead of simply taking the first and last element of the list
		//because that doesn't work for odd counts

		int yLimit = 0, xLimit = -1;

		foreach (int factor in factors)
		{
			int currentHighest = Mathf.Max(yLimit, factor);

			if (currentHighest != yLimit)
			{
				xLimit = yLimit;
				yLimit = currentHighest;
			}
		}

		//Flip axis if more than n positions
		//This is a temporary measure to stop stretched looking inventories
		if (count > flipThreshold)
		{
			(xLimit, yLimit) = (yLimit, xLimit);
		}

		//Need to perform two (one positive, one negative) iterations per each rounded half of the limits
		//we stop immeditely if the number of iterations reaches the limit
		int totalYIterations = 0;
		for (int y = Mathf.FloorToInt(yLimit / 2.0f); y >= 0 && totalYIterations < yLimit; y--)
		{
			//One negative, one positive
			for (int i = -1; i <= 1 && totalYIterations < yLimit; i += 2)
			{
				int totalXIterations = 0;
				for (int x = Mathf.FloorToInt(xLimit / 2.0f); x >= 0 && totalXIterations < xLimit; x--)
				{
					//One negative, one positive
					for (int j = -1; j <= 1 && totalXIterations < xLimit; j += 2)
					{
						//Add output at calculated position
						//Add some extra displacement if the count is an even number (as the there will be no true center position)
						Vector2 anchoredPosition = new Vector2(
							(x * j * (bufferBetween * 2)) - (Mathf.Clamp01(x) * j * (1.0f - (xLimit % 2)) * bufferBetween),
							(y * i * (bufferBetween * 2)) - (Mathf.Clamp01(y) * i * (1.0f - (yLimit % 2)) * bufferBetween)
							);

						//Add to output positions
						//Need to order the positions so they have a defined sensible order
						//This is fairly simple, we simply need to perform a simplistic priority add operation that use the position
						//priority is calculated as so: (y * 1000.0f) + (-1.0f * x).
						//This enforces a "top to bottom, left to right order", as the y is massively weighted and the x is inverted

						float priority = (anchoredPosition.y * 1000.0f) + (-1.0f * anchoredPosition.x);

						//Default first position
						int addIndex = 0;

						for (addIndex = 0; addIndex < toReturn.Count; addIndex++)
						{
							if (priority > prios[addIndex])
							{
								//Higher priority than this position
								//We have found our insert placement
								break;
							}
						}

						toReturn.Insert(addIndex, anchoredPosition);
						prios.Insert(addIndex, priority);

						//Increment total x iterations
						totalXIterations++;
					}
				}

				//Increment total y iterations
				totalYIterations++;
			}
		}

		return toReturn;
	}

	public static bool CloseInteractionBasedUI(Transform targetsTransform, bool overrideNullCheck)
	{
		return 
			(targetsTransform == null && !overrideNullCheck) || 
			(targetsTransform != null && Vector3.Distance(targetsTransform.position, CameraManagement.GetMainCameraPosition()) > Interaction.Ranges.standard) || 
			(targetsTransform != null && !targetsTransform.gameObject.activeSelf) ||
			(targetsTransform == null && PlayerCapitalShip.InJumpTravelStage());
	}
}
