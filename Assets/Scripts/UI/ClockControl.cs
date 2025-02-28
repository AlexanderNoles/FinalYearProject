using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MonitorBreak;

public class ClockControl : MonoBehaviour
{
	private static ClockControl instance;

	public RectTransform dayPivot;
	public RectTransform monthPivot;
	public TextMeshProUGUI dateLabel;

	public GameObject pauseIcon;

	[Header("Follow Through")]
	public RectTransform followThroughEmpty;
	public AnimationCurve followThroughAnimCurve;
	public float followUpSpeed = 2.0f;
	private Quaternion startRot;
	private Quaternion endRot;

	private float postChangeAnimT = 1.0f;

	private void Awake()
	{
		instance = this;
		pauseIcon.SetActive(false);
	}

	public void UpdateClockPosition()
	{
		dateLabel.text = SimulationManagement.GetDateString();

		dayPivot.rotation = Quaternion.Euler(0, 0, -360.0f * SimulationManagement.GetCurrentDayPercentage());
		monthPivot.rotation = Quaternion.Euler(0, 0, -360.0f * SimulationManagement.GetCurrentMonthPercentage());

		startRot = followThroughEmpty.rotation;
		endRot = dayPivot.rotation;

		postChangeAnimT = 0.0f;
	}

	private void Update()
	{
		if (postChangeAnimT < 1.0f)
		{
			postChangeAnimT += Time.deltaTime * SimulationManagement.GetSimulationSpeed();

			followThroughEmpty.rotation = Quaternion.LerpUnclamped(startRot, endRot, followThroughAnimCurve.Evaluate(postChangeAnimT * followUpSpeed));
		}

		if (InputManagement.GetKeyDown(InputManagement.toggleTimePauseKey))
		{
			ToggleTimePause();
		}
	}

	public static void ExternalToggleTimePause()
	{
		if (instance == null)
		{
			return;
		}

		instance.ToggleTimePause();
	}

	public static bool Paused()
	{
		if (instance == null)
		{
			return false;
		}

		return instance.pauseIcon.activeSelf;
	}

	public void ToggleTimePause()
	{
		if (!TimeManagement.RemoveTimeScale(this))
		{
			//Time scale was not removed
			pauseIcon.SetActive(true);

			//Very low priority so it doesn't override pause 
			TimeManagement.AddTimeScale(0.0f, 1, this);
		}
		else
		{
			pauseIcon.SetActive(false);
		}
	}
}
