using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentBattleUIControl : MonoBehaviour
{
	public GameObject target;

	private void Update()
	{
		//Be active if current interaction is attack
		target.SetActive(
			PlayerInteractionManagement.GetCurrentInteraction() != null && 
			PlayerInteractionManagement.GetCurrentInteraction().GetType().Equals(typeof(AttackInteraction)));
	}
}
