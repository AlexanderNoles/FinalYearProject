using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateDateLabel : PostTickUpdate
{
	public TextMeshProUGUI label;

	protected override void PostTick()
	{
		label.text = SimulationManagement.GetDateString();
	}
}
