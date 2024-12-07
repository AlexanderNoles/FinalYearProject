using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipManagementUIControl : MonoBehaviour
{
	public List<GameObject> pageCarousel = new List<GameObject>();
	private int currentCarouselIndex;

	private void Awake()
	{
		foreach (GameObject page in pageCarousel)
		{
			page.SetActive(false);
		}
	}

	private void OnEnable()
	{
		//Reset index
		currentCarouselIndex = -1;

		//Load first page
		UpdateCarousel(0);
	}

	private void UpdateCarousel(int index)
	{
		//Map index to carousel range
		int previousIndex = currentCarouselIndex;

		if (index <= -1)
		{
			index = pageCarousel.Count - 1;
		}

		currentCarouselIndex = index % pageCarousel.Count;


		if (previousIndex != currentCarouselIndex)
		{
			if (previousIndex != -1)
			{
				pageCarousel[previousIndex].SetActive(false);
			}

			pageCarousel[currentCarouselIndex].SetActive(true);
		}
	}

	public void NextPage()
	{
		UpdateCarousel(currentCarouselIndex + 1);
	}

	public void PreviousPage()
	{
		UpdateCarousel(currentCarouselIndex - 1);
	}
}
