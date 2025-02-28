using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseManagement : MonoBehaviour
{
	private static MouseManagement instance;

	[System.Serializable]
	public class MouseState
	{
		public Sprite mouseImage;
		public Sprite tagALongImage = null;
		//Natural conversion from standard pixels per unit to ui's pixels per unit
		public float tagALongScale = 16.0f / 256.0f;
		public bool hidden = false;
	}

	private MouseState currentMouseState = null;
	private MouseState backupMouseState;
	private Vector2 additionalMouseOffset;

	public CanvasScaler scaler;
	public Sprite basicMouseImage;
	public Image mouseImage;
	private GameObject mouseGO;
	private RectTransform mouseRect;
	public Image tagAlongImage;
	private GameObject tagGO;
	private RectTransform tagRect;
	[Header("Mouse Click Effect")]
	public RectTransform mouseClickEffectRect;
	public Image mouseClickImage;
	private Material mouseClickMat;
	private float mouseClickT;
	private float clickBuffer;
	public AnimationCurve mouseClickAnimCurve;

	public static void ApplyMouseState(MouseState newMouseState)
	{
		instance.currentMouseState = newMouseState;
		ReloadMouseState();
	}

	public static void ResetMouseState()
	{
		instance.currentMouseState = null;
		ReloadMouseState();
	}

	public static void ReloadMouseState()
	{
		instance.ReloadMouseStateInternal();
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
		{
			return;
		}

		//When game has grabbed focus back reset mouse state
		ReloadMouseState();
	}

	private void ReloadMouseStateInternal()
	{
		//Change mouse to current mouse state
		//If mouse state is null apply the backup mouse state
		if (currentMouseState == null)
		{
			ApplyMouseState(backupMouseState);
			return;
		}

		if (currentMouseState.hidden)
		{
			mouseGO.SetActive(false);
			return;
		}
		else
		{
			mouseGO.SetActive(true);
		}

		mouseImage.sprite = currentMouseState.mouseImage;
		mouseImage.SetNativeSize();

		//Calculate additional offset based on the rect size
		//This will mean the top right corner of the mouse image
		//will appear where the click input position is
		Vector2 sizeDelta = mouseRect.sizeDelta;
		additionalMouseOffset = new Vector2(sizeDelta.x, -sizeDelta.y) / 2.0f;

		//Setup tagalong image
		tagGO.SetActive(currentMouseState.tagALongImage != null);
		if (tagGO.activeSelf)
		{
			tagAlongImage.sprite = currentMouseState.tagALongImage;
			tagAlongImage.SetNativeSize();

			tagRect.sizeDelta *= currentMouseState.tagALongScale;

			tagRect.anchoredPosition = new Vector2(tagRect.sizeDelta.x, -tagRect.sizeDelta.y) / 2.0f;
			tagRect.anchoredPosition += new Vector2(-10, 0);
		}
		//

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Confined;
	}

	private void Awake()
	{
		instance = this;

		mouseClickMat = mouseClickImage.material;
		mouseClickMat.SetFloat("_T", 1.0f);

		mouseGO = mouseImage.gameObject;
		mouseRect = mouseGO.transform as RectTransform;

		tagGO = tagAlongImage.gameObject;
		tagRect = tagAlongImage.transform as RectTransform;

		//Setup backup mouse state
		backupMouseState = new MouseState();
		backupMouseState.mouseImage = basicMouseImage;

		//Reload mouse state
		//If no current mouse state
		//backup will be assumed
		ReloadMouseState();
	}

	private void Update()
	{
		if (currentMouseState.hidden)
		{
			return;
		}

		//Move mouse to position
		mouseRect.anchoredPosition = (InputManagement.GetMousePosition() + additionalMouseOffset) * (1.0f / scaler.scaleFactor);
		mouseClickEffectRect.anchoredPosition = InputManagement.GetMousePosition() * (1.0f / scaler.scaleFactor);

		//Mouse on click effect
		if (mouseClickT > 0.0f)
		{
			mouseClickT -= Time.unscaledDeltaTime * 9.0f;
			mouseClickT = Mathf.Clamp01(mouseClickT);
			mouseClickMat.SetFloat("_T", mouseClickAnimCurve.Evaluate(1.0f - mouseClickT));
		}
		else if (InputManagement.GetMouseButton(InputManagement.MouseButton.Left))
		{
			clickBuffer += Time.deltaTime;
		}
		else if (InputManagement.GetMouseButtonUp(InputManagement.MouseButton.Left))
		{
			if (clickBuffer <= 0.2f)
			{
				//Only clicked mouse button didn't hold it
				mouseClickT = 1.0f;
			}

			clickBuffer = 0.0f;
		}
	}
}
