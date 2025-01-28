using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCollectionDrawer
{
	public ShipCollection target = null;
	public Transform parent;
	private SimulationEntity entity;
	private int targetLTUI = -1;
	public List<ShipSimObjectBehaviour> drawnShips = new List<ShipSimObjectBehaviour>();

	public void Link(ShipCollection newTarget, SimulationEntity entity)
	{
		target = newTarget;
		this.entity = entity;
	}

	public void UnLink()
	{
		Link(null, null);
		UndrawAll();
	}

	public void DrawShips()
	{
		if (target == null)
		{
			return;
		}

		//Undraw all currently drawn ships
		UndrawAll();

		//Draw ships
		foreach (Ship ship in target.GetShips())
		{
			DrawShip(ship);
		}

		//Mark as displaying current collection state
		targetLTUI = target.lastTickUpdateID;
	}

	public void UndrawAll()
	{
		foreach (ShipSimObjectBehaviour sh in drawnShips)
		{
			UndrawShip(sh, false);
		}

		//Stop tracking all
		drawnShips.Clear();
	}

	public void DrawShip(Ship ship)
	{
		//Create an instance of this ship in the scene as a ship drawer
		//Link that drawer to this ship

		//Set position as vector3.zero, ship drawer should figure out position on it's own
		//to keep things dynamic and flexible
		ShipSimObjectBehaviour newShip = GeneratorManagement.DrawShip(Vector3.zero);
		newShip.SetParent(parent);
		newShip.Link(ship);

		newShip.Init(this);

		//Start tracking ship drawer
		drawnShips.Add(newShip);
	}

	public void UndrawShip(ShipSimObjectBehaviour shipDrawer, bool stopTracking)
	{
		GeneratorManagement.ReturnShip(shipDrawer);

		if (stopTracking)
		{
			//Stop tracking ship drawer
			drawnShips.Remove(shipDrawer);
		}
	}

	public void Update()
	{
		if (target.lastTickUpdateID > targetLTUI)
		{
			//This target has been updated
			targetLTUI = target.lastTickUpdateID;

			//Reflect the changes
			List<Ship> toRemove = new List<Ship>();
			foreach ((ShipCollection.UpdateType, Ship) entry in target.recordedUpdates)
			{
				if (entry.Item1 == ShipCollection.UpdateType.Add)
				{
					DrawShip(entry.Item2);
				}
				else if (entry.Item1 == ShipCollection.UpdateType.Remove)
				{
					toRemove.Add(entry.Item2);
				}
			}

			for (int i = 0; i < drawnShips.Count && toRemove.Count > 0; i++)
			{
				int index = toRemove.IndexOf(drawnShips[i].target as Ship);

				if (index != -1)
				{
					UndrawShip(drawnShips[i], true);
					toRemove.RemoveAt(index);
					i--;
				}
			}
		}
	}
}
