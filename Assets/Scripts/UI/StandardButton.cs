using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StandardButton : MonoBehaviour
{
	public Button target;

	public void Enable(bool enabled)
	{
		target.interactable = enabled;
	}
}
