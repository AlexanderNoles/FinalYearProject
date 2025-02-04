using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fleet : ShipCollection
{
	public List<Ship> ships = new List<Ship>();

	public override List<Ship> GetShips()
	{
		return ships;
	}

	public override Ship GetNewShip()
	{
		return new FleetShip();
	}

	public override void AddShip(Ship newShip)
	{
		ships.Add(newShip);
	}
}
