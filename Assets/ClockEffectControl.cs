using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClockEffectControl : MonoBehaviour
{
	public RectTransform dayPivot;
	public RectTransform monthPivot;
	public TextMeshProUGUI dateLabel;

	[Header("Follow Through")]
	public RectTransform followThroughEmpty;
	public AnimationCurve followThroughAnimCurve;
	public float followUpSpeed = 2.0f;
	private Quaternion startRot;
	private Quaternion endRot;

	private float postChangeAnimT = 1.0f;

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
	}
}
