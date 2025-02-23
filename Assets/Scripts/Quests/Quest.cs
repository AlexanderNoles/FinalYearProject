using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest : IDisplay
{
	public VisitableLocation questOrigin;

	public virtual bool PostTickValidate()
	{
		return true;
	}

	public virtual bool CompletedCheck()
	{
		//By default auto complete quest if you are within N in-engine units of the target position
		RealSpacePosition targetPos = GetTargetPosition();

		if (targetPos != null)
		{
			double distance = targetPos.Distance(WorldManagement.worldCenterPosition);

			if (distance < 200.0f * WorldManagement.invertedInEngineWorldScaleMultiplier)
			{
				return true;
			}
		}

		return false;
	}

	public virtual void ApplyReward()
	{
		//By default just apply the currency reward
		if (PlayerManagement.PlayerEntityExists())
		{
			PlayerInventory playerInventory = PlayerManagement.GetInventory();

			playerInventory.AdjustCurrency(CurrencyRewardAmount());
		}
	}

	public virtual float CurrencyRewardAmount()
	{
		return 100.0f;
	}

	public virtual RealSpacePosition GetTargetPosition()
	{
		return null;
	}

	//Display methods
	public virtual string GetDescription()
	{
		throw new System.NotImplementedException();
	}

	public virtual string GetExtraInformation()
	{
		throw new System.NotImplementedException();
	}

	public virtual Sprite GetIcon()
	{
		throw new System.NotImplementedException();
	}

	public virtual string GetTitle()
	{
		throw new System.NotImplementedException();
	}
}
