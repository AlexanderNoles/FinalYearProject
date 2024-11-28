using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeShip : Ship
{
    public float startTime;
    public float currentJourneysLength;

    public Location homeLocation;
    public Location tradeTarget = null;

    public int cargoAmount = 0;

    public void StartNewJourney(Location newTarget, float journeyLength, int cargoAmount)
    {
        tradeTarget = newTarget;
        startTime = SimulationManagement.GetCurrentSimulationTime();

        currentJourneysLength = journeyLength;
        this.cargoAmount = cargoAmount;

		//Repair ship
		health = 10;
    }
}
