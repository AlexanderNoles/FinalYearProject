using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableWindow : MonoBehaviour, IDragHandler, IBeginDragHandler
{
	public RectTransform target;
	private Vector2 lastPos;
	private float scaleFactor;
	public Vector2 additionalInitialOffset;
	public CanvasScaler targetCanvasScaler;

	private void Awake()
	{
		scaleFactor = 1f / targetCanvasScaler.scaleFactor;
	}

	public void InitialOffset()
	{
		target.anchoredPosition = 
			(InputManagement.GetMousePosition() * scaleFactor) + 
			(new Vector2(target.sizeDelta.x, -target.sizeDelta.y) / 2) +
			additionalInitialOffset;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		lastPos = eventData.position;
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector2 offsetThisFrame = eventData.position - lastPos;

		if (offsetThisFrame.magnitude > 0)
		{
			lastPos = eventData.position;
			target.anchoredPosition3D += new Vector3(offsetThisFrame.x, offsetThisFrame.y, 0.0f) * scaleFactor;
		}
	}
}
