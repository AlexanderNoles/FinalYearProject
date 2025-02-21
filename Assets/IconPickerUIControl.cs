using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class IconPickerUIControl : MonoBehaviour
{
	public GameObject buttonBase;

	public UnityEvent<Sprite> onSpritePicked = new UnityEvent<Sprite>();

	private void Start()
	{
		List<Sprite> possibleSprites = VisualDatabase.GetAllFactionSprites();
		List<Vector3> positions = UIHelper.CalculateRowedPositions(possibleSprites.Count, new Vector3(15, -15), new Vector3(30, 0), new Vector3(0, -30));

		for (int i = 0; i < positions.Count; i++)
		{
			Sprite sprite = possibleSprites[i];

			RectTransform newButton = Instantiate(buttonBase, transform).transform as RectTransform;
			newButton.anchoredPosition3D = positions[i];

			newButton.GetComponent<Image>().sprite = sprite;
			newButton.GetComponent<Button>().onClick.AddListener(() => { ButtonCallback(sprite); });
		}
	}

	public void ButtonCallback(Sprite sprite)
	{
		onSpritePicked.Invoke(sprite);
	}
}
