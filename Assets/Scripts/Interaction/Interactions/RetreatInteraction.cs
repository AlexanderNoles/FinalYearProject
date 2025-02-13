using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetreatInteraction : Interaction
{
	public override bool ValidateOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		if (target.baseLocation == null || PlayerCapitalShip.IsJumping())
		{
			return false;
		}

		if (target.baseLocation is GlobalBattleData.Battle)
		{
			GlobalBattleData.Battle battle = target.baseLocation as GlobalBattleData.Battle;

			if (battle.GetInvolvedEntities().Contains(PlayerManagement.GetTarget().id))
			{
				//Player is involved in this battle
				return true;
			}
		}

		return false;
	}

	public override void ProcessOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		//We know validate check passed so just convert to battle
		GlobalBattleData.Battle battle = target.baseLocation as GlobalBattleData.Battle;
		RetreatFromBattle(battle);
	}

	private void RetreatFromBattle(GlobalBattleData.Battle battle)
	{
		//Should include an alterante path here for displaying an "error" message on screen that says we cannot retreat so soon after a battle starts!

		//

		RealSpacePosition position = battle.GetPosition();

		//Remove player from battle
		battle.RemoveInvolvedEntity(PlayerManagement.GetTarget().id);

		//Shift all ships at position back to reserves
		MilitaryData militaryData = PlayerManagement.GetMilitary();

		if (militaryData.positionToFleets.ContainsKey(position))
		{
			int count = militaryData.positionToFleets[position].Count;

			for (int i = 0; i < count; i++)
			{
				ShipCollection currentFleet = militaryData.RemoveFleet(position);

				if (currentFleet != null)
				{
					militaryData.AddFleetToReserves(currentFleet);
				}
			}
		}
	}

	public override bool ValidateBehaviour(SimObjectBehaviour interactable)
	{
		//Is this a battle?
		if (interactable.target == null || interactable.target is not GlobalBattleData.Battle || PlayerCapitalShip.InJumpTravelStage())
		{
			return false;
		}

		//Is the player in this battle?
		if ((interactable.target as GlobalBattleData.Battle).GetInvolvedEntities().Contains(PlayerManagement.GetTarget().id))
		{
			return true;
		}

		return false;
	}

	public override void ProcessBehaviour(SimObjectBehaviour interactable)
	{
		GlobalBattleData.Battle battle = interactable.target as GlobalBattleData.Battle;
		RetreatFromBattle(battle);
	}

	public override float GetRange()
	{
		return Ranges.infinity;
	}

	public override int GetDrawPriority()
	{
		return 80;
	}

	protected override string GetIconPath()
	{
		return "retreatInteraction";
	}
}
