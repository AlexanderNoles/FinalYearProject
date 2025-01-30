using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Static script to help centralize balance
public static class BalanceManagement
{
	public const float killWorthRatio = 1f;
	public const float mineralDepositWorthRatio = 0.5f;

	//This acts as a general modifier to attack damage and regen basically
	//Might include other things in the future
	//(28/01/2025)
	public const float overallBattlePace = 0.4f;

	//How much dealing damage to entites affects your reputation?
	//Realisitcally one attack should make them hate you but I want to give players a bit of leeway
	public const float damageReputationRatio = 0.05f;
}