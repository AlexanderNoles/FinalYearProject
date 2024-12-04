using System.Collections.Generic;

public class PlayerShipData : DataBase
{
	//Information about the general state
	public float amenities = 5;


	//Specific units contained on the ship
	//Like factories or ship bays or population centers, etc.
	List<PlayerShipUnitBase> shipUnits = new List<PlayerShipUnitBase>();

	//Perks data (Maybe this should be seperated?)
}