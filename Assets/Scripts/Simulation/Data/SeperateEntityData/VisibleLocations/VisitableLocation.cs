using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisitableLocation : Location, IDisplay
{
	public virtual void InitDraw()
	{

	}

	public virtual void Cleanup()
	{

	}

	public virtual void Draw()
	{

	}

	public virtual float GetEntryOffset()
	{
		return 0.0f;
	}


	//UI Display methods
	public virtual string GetTitle()
	{
		return "Unkown";
	}

	public virtual string GetDescription()
	{
		return "A location.";
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

	//Shop
	public virtual bool HasShop()
	{
		return false;
	}

	public virtual Shop GetShop()
	{
		return null;
	}

	//Fuel
	public virtual bool CanBuyFuel()
	{
		return false;
	}

	public virtual float FuelPerMoneyUnit()
	{
		return 1.0f;
	}
}
