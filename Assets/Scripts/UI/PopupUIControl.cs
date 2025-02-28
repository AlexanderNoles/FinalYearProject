using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupUIControl : MonoBehaviour
{
	private static PopupUIControl instance;
	public GameObject mainUI;
	private RectTransform mainUIRect;
	private static UnityAction backup;

	public TextMeshProUGUI titleText;
	public TextMeshProUGUI contentText;
	public CanvasScaler scaler;
	private float scaleFactor;

	private UnityAction onAccept = null;
	private UnityAction onDeny = null;

	private void Awake()
	{
		instance = this;
		mainUIRect = mainUI.GetComponent<RectTransform>();
		scaleFactor = 1f / scaler.scaleFactor;

		//By default just close this menu
		backup = () => 
		{
			Close();
		};

		//Close the ui if it is open
		//UI could be kept open for testing/visual design reasons
		Close();
	}

	public static void SetActive(string title, string text, UnityAction onAccept = null, UnityAction onDeny = null)
	{
		//Replace on call with backups
		onAccept ??= backup;
		onDeny ??= backup;

		instance.mainUI.SetActive(true);
		instance.titleText.text = title;
		instance.contentText.text = text;

		instance.onAccept = onAccept; 
		instance.onDeny = onDeny;

		//Set position to center of screen
		instance.mainUIRect.anchoredPosition3D = new Vector3(Screen.width / 2.0f, Screen.height / 2.0f) * instance.scaleFactor;

		if (!ClockControl.Paused())
		{
			ClockControl.ExternalToggleTimePause();
		}
	}

	public void TriggerAccept()
	{
		onAccept?.Invoke();

		if (mainUI.activeSelf)
		{
			Close();
		}
	}

	public void TriggerDeny()
	{
		onDeny?.Invoke();

		if (mainUI.activeSelf)
		{
			Close();
		}
	}

	public void Close()
	{
		mainUI.SetActive(false);

		if (ClockControl.Paused())
		{
			ClockControl.ExternalToggleTimePause();
		}
	}
}
