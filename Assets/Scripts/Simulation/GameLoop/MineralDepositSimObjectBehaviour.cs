using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralDepositSimObjectBehaviour : SimObjectBehaviour
{
	protected override TakenDamageResult TakeDamage(float rawDamageNumber, BattleBehaviour origin)
	{
		TakenDamageResult result = base.TakeDamage(rawDamageNumber, origin);

		if (PlayerSimObjBehaviour.IsPlayerBB(origin) && PlayerManagement.PlayerEntityExists())
		{
			//Give player currency equivalent to damage delt
			//This means mining ability scales with attack power
			PlayerInventory playerInventory = PlayerManagement.GetInventory();
			playerInventory.AdjustCurrency(result.damageTaken * BalanceManagement.mineralDepositWorthRatio);
		}

		return result;
	}
}
