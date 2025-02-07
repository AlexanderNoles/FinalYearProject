using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OnHoverInformationDisplay : MonoBehaviour
{
	private static OnHoverInformationDisplay instance;
	private IDisplay currentTarget;
	private GameObject hoverSource;

	private RectTransform rootRectTransform;

	public GameObject mainUI;
	private RectTransform anchorRect;
	private Vector2 cachedAnchoredPosition;
	public TextMeshProUGUI titleLabel;
	public TextMeshProUGUI descLabel;
	public TextMeshProUGUI extraLabel;

	private void Awake()
	{
		instance = this;
		Hide();

		rootRectTransform = transform as RectTransform;
		anchorRect = mainUI.transform as RectTransform;
		cachedAnchoredPosition = anchorRect.anchoredPosition;
	}

	public static void RemoveIfSource(GameObject potentialSource)
	{
		if (instance.hoverSource == potentialSource)
		{
			instance.Hide();
		}
	}

	public static void SetCurrentTarget(IDisplay newTarget, GameObject hoverSource)
	{
		if (instance.currentTarget == newTarget)
		{
			return;
		}

		instance.currentTarget = newTarget;
		instance.hoverSource = hoverSource;
		instance.Draw();
	}

	public static void RemoveCurrentTarget(IDisplay oldTarget)
	{
		if (instance.currentTarget.Equals(oldTarget))
		{
			//Currently displaying this 
			instance.Hide();
		}
	}

	private void Draw()
	{
		mainUI.SetActive(true);

		titleLabel.text = currentTarget.GetTitle();
		descLabel.text = currentTarget.GetDescription();
		extraLabel.text = currentTarget.GetExtraInformation();
	}

	private void Hide()
	{
		mainUI.SetActive(false);

		//Reset data
		currentTarget = null;
		hoverSource = null;
	}

	private void LateUpdate()
	{
		if (mainUI.activeSelf)
		{
			if (hoverSource != null && !hoverSource.activeInHierarchy)
			{
				//This occurs when the object being hovered over is set inactive
				//Which means it won't have the chance to run the PointerExit function
				Hide();
			}

			//Make sure ui element is always visible
			//By default use the typical pos
			//If no valid pos is found this one will also be used
			Vector3 finalPos = cachedAnchoredPosition;

			//Get current screen dimensions
			Vector2 screenDimensions = new Vector2(Screen.width, Screen.height);

			foreach (Vector2 centerOffset in GenerationUtility.diagonalOffsets2D)
			{
				Vector2 anchorPosThisIteration = new Vector2(cachedAnchoredPosition.x * centerOffset.x, cachedAnchoredPosition.y * centerOffset.y);

				Vector2 worldPos = rootRectTransform.position;
				worldPos.x += anchorPosThisIteration.x;
				worldPos.y += anchorPosThisIteration.y;

				if (CornerCheck(worldPos, anchorRect, screenDimensions))
				{
					//All corners are inside the screen
					finalPos = anchorPosThisIteration;
					break;
				}
			}

			anchorRect.anchoredPosition = finalPos;
		}
	}

	private bool CornerCheck(Vector2 worldSpaceCheckPos, RectTransform targetRect, Vector2 screenDimensions)
	{
		//Check if any corner is outside the screen
		//First get positive offset
		Vector2 offset = targetRect.sizeDelta / 2;

		foreach (Vector2 corner in GenerationUtility.diagonalOffsets2D)
		{
			Vector2 offsetThisIteration = new Vector2(corner.x * offset.x, corner.y * offset.y);
			Vector2 cornerWorldSpacePos = worldSpaceCheckPos + offsetThisIteration;

			//Does this corner fall outside the screen
			bool outsideX = cornerWorldSpacePos.x < 0 || cornerWorldSpacePos.x > screenDimensions.x;
			bool outsideY = cornerWorldSpacePos.y < 0 || cornerWorldSpacePos.y > screenDimensions.y;

			if (outsideX || outsideY)
			{
				return false;
			}
		}

		return true;
	}
}
