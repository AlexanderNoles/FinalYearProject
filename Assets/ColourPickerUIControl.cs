using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColourPickerUIControl : MonoBehaviour
{
	public GameObject buttonBase;

	public UnityEvent<Color> onColourPicked = new UnityEvent<Color>();

	private void Start()
	{
		//Generate all neccesary colour picking buttons
		//Generated at runtime to ensure parity with the visual database
		//Also it's simplier

		List<Color> possibleColours = VisualDatabase.GetAllColours();
		List<Vector3> positions = UIHelper.CalculateRowedPositions(possibleColours.Count, new Vector3(15, -15), new Vector3(30, 0), new Vector3(0, -30));

		for (int i = 0; i < positions.Count; i++)
		{
			Color colour = possibleColours[i];

			RectTransform newButton = Instantiate(buttonBase, transform).transform as RectTransform;
			newButton.anchoredPosition3D = positions[i];

			newButton.GetComponent<Image>().color = colour;
			Color clone = new Color(colour.r, colour.g, colour.b);
			newButton.GetComponent<Button>().onClick.AddListener(() => { ButtonCallback(clone); });
		}
	}

	public void ButtonCallback(Color color)
	{
		onColourPicked.Invoke(color);
	}
}
