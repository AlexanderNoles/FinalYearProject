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
			//Give player currency correlated to damage dealt
			//This means mining ability scales with attack power
			PlayerInventory playerInventory = PlayerManagement.GetInventory();
			//Limit currency gain to reduce later viability of mineral deposits as places to earn currency frrom mining
			playerInventory.AdjustCurrency(MathHelper.ValueTanhFalloff(result.damageTaken * BalanceManagement.mineralDepositWorthRatio, 5, -1));
		}

		return result;
	}
}
