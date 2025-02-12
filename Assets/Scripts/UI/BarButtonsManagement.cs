using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarButtonsManagement : MonoBehaviour
{
	public List<GameObject> targetBarButtons = new List<GameObject>();

	private void Update()
	{
		bool showBarButtons = UIManagement.InPureNeutral();

		foreach (GameObject targetBarButton in targetBarButtons)
		{
			targetBarButton.SetActive(showBarButtons);
		}
	}
}
