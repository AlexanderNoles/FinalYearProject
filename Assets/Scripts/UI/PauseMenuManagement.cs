using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuManagement : UIState
{
	public override KeyCode GetSetActiveKey()
	{
		return InputManagement.togglePauseMenu;
	}

	public GameObject target;

	protected override GameObject GetTargetObject()
	{
		return target;
	}

	public void ReturnToStartMenu()
	{
		GameManagement.LoadScene(0);
	}
}
