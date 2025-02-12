using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//15 apart
public class SectionedBarIndicatorUI : MonoBehaviour
{
	public GameObject slotBaseObject;
	private List<Image> slots = new List<Image>();

	public void Draw(int current, int max)
	{
		current = Mathf.Min(current, max);

		//Have i at a higher scoper than just the loop so we can check if any remaining slots need to be hidden
		int i = 0;

		for (; i < max; i++)
		{
			if (i >= slots.Count)
			{
				//Add new slot
				RectTransform rect = Instantiate(slotBaseObject, transform).transform as RectTransform;
				rect.anchoredPosition3D = new Vector3(i * 15, 0, 0);

				slots.Add(rect.GetComponent<Image>());
			}
			else
			{
				slots[i].gameObject.SetActive(true);
			}

			//Set colour based on whether below or above current
			slots[i].color = i < current ? Color.white : Color.grey;
		}

		for (; i < slots.Count; i++)
		{
			slots[i].gameObject.SetActive(false);
		}
	}
}
