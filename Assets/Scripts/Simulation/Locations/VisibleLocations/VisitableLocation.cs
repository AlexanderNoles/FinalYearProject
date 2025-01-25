using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VisitableLocation : Location, IDisplay
{
	private static Dictionary<Transform, LocationContextLink> transformToContextLink = new Dictionary<Transform, LocationContextLink>();
	
	public bool HasHealth()
	{
		return GetMaxHealth() != -1;
	}

	public virtual float GetMaxHealth()
	{
		return -1;
	}

	public virtual void OnDeath()
	{

	}

	public virtual int GetEntityID()
	{
		return -1;
	}

	public virtual List<WeaponBase> GetWeapons()
	{
		return new List<WeaponBase>();
	}

	protected void ApplyContext(Transform target)
	{
		if (!transformToContextLink.ContainsKey(target))
		{
			if (target.TryGetComponent(out LocationContextLink component))
			{
				transformToContextLink[target] = component;
			}
		}

		transformToContextLink[target].SetContext(this);
	}

	public virtual void InitDraw(Transform parent)
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
		RealSpacePosition pos = GetPosition();

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

	public virtual bool FlashOnMap()
	{
		return false;
	}

	public virtual Color GetFlashColour()
	{
		return Color.red;
	}

	//Shop
	public bool HasShop()
	{
		return GetShop() != null;
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
