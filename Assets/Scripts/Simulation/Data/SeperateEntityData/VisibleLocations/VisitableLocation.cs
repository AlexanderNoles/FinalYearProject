using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VisitableLocation : Location, IDisplay
{
	public virtual void InitDraw(Transform parent)
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
		RealSpacePostion pos = GetPosition();

		return $"Coordinates: (X:{Math.Round(pos.x)}, Y:{Math.Round(pos.z)})";
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
