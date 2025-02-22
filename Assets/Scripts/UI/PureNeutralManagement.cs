using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PureNeutralManagement : MonoBehaviour
{
	public List<GameObject> toHideImpure = new List<GameObject>();

	private void Update()
	{
		bool showBarButtons = UIManagement.InPureNeutral();

		foreach (GameObject targetBarButton in toHideImpure)
		{
			targetBarButton.SetActive(showBarButtons);
		}
	}
}
