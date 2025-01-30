using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

//Base class used to represent an "object" in the simulation, very general, mainly used so runtime scripts can correctly interface with the simulation
public class SimObject : IDisplay
{
	//Optimization method to avoid multiple get component calls for same object
	private static Dictionary<Transform, SimObjectBehaviour> transToSOB = new Dictionary<Transform, SimObjectBehaviour>();
	private EntityLink parent;

	public void SetParent(EntityLink newParent)
	{
		parent = newParent;
	}

	public virtual int GetEntityID()
	{
		if (parent != null)
		{
			return parent.Get().id;
		}

		return -1;
	}

	public void LinkToBehaviour(Transform target)
	{
		if (!transToSOB.ContainsKey(target))
		{
			if (target.TryGetComponent(out SimObjectBehaviour targetScript))
			{
				transToSOB[target] = targetScript;
			}
		}

		transToSOB[target].Link(this);
	}

	// Draw System //
	public virtual void InitDraw(Transform parent, PlayerLocationManagement.DrawnLocation drawnLocation)
	{

	}

	public virtual void Cleanup()
	{

	}

	public virtual void DrawUpdate()
	{

	}

	public virtual void DrawUpdatePostTick()
	{

	}

	// Battle System Interaction //
	public virtual float GetMaxHealth()
	{
		return 100.0f;
	}

	public virtual void OnDeath()
	{

	}

	public virtual List<StandardSimWeaponProfile> GetWeapons()
	{
		return new List<StandardSimWeaponProfile>();
	}

	public virtual float GetKillReward()
	{
		return 0.0f;
	}

	// Interaction System Helpers //
	public bool HasShop()
	{
		return GetShop() != null;
	}

	public virtual Shop GetShop()
	{
		return null;
	}

	public virtual bool CanBuyFuel()
	{
		return false;
	}

	public virtual float FuelPerMoneyUnit()
	{
		return 1.0f;
	}

	// Reputation System //
	public bool ReputationEnabled()
	{
		//Has a parent and that parent has feelings data
		return parent != null && parent.Get().HasData(DataTags.Feelings);
	}

	public void SetPlayerReputation(float newReputation)
	{
		if (!ReputationEnabled())
		{
			return;
		}

		//Get player id
		if (PlayerManagement.PlayerEntityExists())
		{
			int playerID = PlayerManagement.GetTarget().id;

			//Clamp reputation to expected range
			newReputation = Mathf.Clamp(newReputation, -1.0f, 1.0f);

			//If reputation enabled check was passed we should have feelings data
			Assert.IsTrue(parent.Get().GetData(DataTags.Feelings, out FeelingsData feelingsData));

			//Does this entity know about the player?
			//If not add it to their feelings data
			if (!feelingsData.idToFeelings.ContainsKey(playerID))
			{
				feelingsData.idToFeelings.Add(playerID, new FeelingsData.Relationship(newReputation));
			}
			else 
			{
				feelingsData.idToFeelings[playerID].favourability = newReputation;
			}
		}
	} 

	public void AdjustPlayerReputation(float adjustment)
	{
		if (!ReputationEnabled())
		{
			return;
		}

		//Get player id
		if (PlayerManagement.PlayerEntityExists())
		{
			int playerID = PlayerManagement.GetTarget().id;
			//If reputation enabled check was passed we should have feelings data
			Assert.IsTrue(parent.Get().GetData(DataTags.Feelings, out FeelingsData feelingsData));

			//Do we not know about the player?
			if (!feelingsData.idToFeelings.ContainsKey(playerID))
			{
				feelingsData.idToFeelings.Add(playerID, new FeelingsData.Relationship(Mathf.Clamp(adjustment, -1.0f, 1.0f)));
			}
			else
			{
				feelingsData.idToFeelings[playerID].favourability = Mathf.Clamp(feelingsData.idToFeelings[playerID].favourability + adjustment, -1.0f, 1.0f);
			}
		}
	}

	// UI Display Methods //
	//Part of IDisplay, allows certain ui elements to expect things they can draw from when handed an object
	public virtual string GetTitle()
	{
		return "Unkown";
	}

	public virtual string GetDescription()
	{
		return string.Empty;
	}

	public virtual Sprite GetIcon()
	{
		return null;
	}

	public virtual string GetExtraInformation()
	{
		return "";
	}

	public virtual Color GetMapColour()
	{
		return Color.white;
	}

	public virtual bool FlashOnMap()
	{
		return false;
	}

	public virtual Color GetFlashColour()
	{
		return Color.red;
	}
}
