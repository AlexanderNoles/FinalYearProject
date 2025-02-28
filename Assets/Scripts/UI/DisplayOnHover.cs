using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DisplayOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public IDisplay target;
	public bool mini;

	public void Setup(IDisplay target)
	{
		this.target = target;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		//Tell display ui to active with our target
		if (target != null)
		{
			OnHoverInformationDisplay.SetCurrentTarget(target, gameObject, mini);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		//Tell display ui to close if it is still displaying our target
		//Class will decide whether to stop displaying or not
		if (target != null)
		{
			OnHoverInformationDisplay.RemoveCurrentTarget(target);
		}
	}
}
