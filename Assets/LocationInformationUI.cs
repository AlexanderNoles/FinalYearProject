using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationInformationUI : MonoBehaviour
{
	private void OnEnable()
	{
		PlayerLocationManagement.onLocationChanged.AddListener(Draw);

		//Run initial draw incase location changed while this ui element was not active
		Draw();
	}

	private void OnDisable()
	{
		PlayerLocationManagement.onLocationChanged.RemoveListener(Draw);
	}

	public void Draw()
	{

	}
}
