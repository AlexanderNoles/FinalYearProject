using System;
using System.Collections.Generic;

public class PlayerShipData : DataBase
{
	//Information about the general state
	public float amenities = 5;


	//Specific units contained on the ship
	//Like factories or ship bays or population centers, etc.
	public List<PlayerShipUnitBase> shipUnits = new List<PlayerShipUnitBase>();

	public int CountUnit<T>()
	{
		int count = 0;

		foreach (PlayerShipUnitBase unit in shipUnits)
		{
			if (unit is T)
			{
				count++;
			}
		}

		return count;
	}

	//Perks data (Maybe this should be seperated?)
}