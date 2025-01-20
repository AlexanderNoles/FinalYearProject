using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LocationInformationUI : MonoBehaviour
{
	public GameObject blocker;
	public InformationDisplayControl informationDisplayControl;
	private VisitableLocation cachedLocation;

	[Header("Shop")]
	public StandardButton shopButton;
	private Shop shopData;
	public ShopUIControl shopControl;
	private FloatingWindow shopWindow;

	[Header("Fuel")]
	public StandardButton fuelButton;
	private bool currentLocationCanFuel;
	private float cachedFuelPerMoneyUnit;

	private void Awake()
	{
		shopWindow = shopControl.GetComponent<FloatingWindow>();
	}

	private void OnEnable()
	{
		PlayerLocationManagement.onLocationChanged.AddListener(Draw);

		//Run initial draw incase location changed while this ui element was not active
		Draw();
	}

	private void OnDisable()
	{
		PlayerLocationManagement.onLocationChanged.RemoveListener(Draw);
	}

	public void Draw()
	{
		//Get current location
		VisitableLocation currentLocation = PlayerLocationManagement.GetPrimaryLocation();
		cachedLocation = currentLocation;

		if (currentLocation == null)
		{
			//Set blocker active
			blocker.SetActive(true);
		}
		else
		{
			//Get information about current location and write it to the UI
			//Turn off the blocker if it is on
			blocker.SetActive(false);

			//Draw information
			informationDisplayControl.Draw(currentLocation);

			//Setup buttons
			if (currentLocation.HasShop())
			{
				shopData = currentLocation.GetShop();
				shopButton.Enable(true);
			}
			else
			{
				shopButton.Enable(false);
			}

			currentLocationCanFuel = currentLocation.CanBuyFuel();

			if (currentLocationCanFuel)
			{
				cachedFuelPerMoneyUnit = currentLocation.FuelPerMoneyUnit();
			}
		}
	}

	public void BuyFuel()
	{
        //Get player inventory
        PlayerInventory playerInventory = PlayerManagement.GetInventory();

        const float maxFuelBuyRate = 50;
        float moneyToSpend = Mathf.Min(maxFuelBuyRate, playerInventory.mainCurrency);

        if (moneyToSpend > 0)
        {
            //Can afford any fuel
            //Subtract money
            playerInventory.mainCurrency -= moneyToSpend;
            playerInventory.fuel += moneyToSpend * cachedFuelPerMoneyUnit;

            //Redraw main ui
            MainInfoUIControl.ForceCurrencyRedraw();
        }
    }

	private void Update()
	{
		if (blocker.activeSelf)
		{
			return;
		}

		//If jumping don't allow refuel
		//This is because it interfers with the fuel count down during jump effect
		//Also it would be weird to buy fuel will about to jump
		fuelButton.Enable(currentLocationCanFuel && !PlayerCapitalShip.IsJumping());

		if (fuelButton.Enabled() && InputManagement.GetKeyDown(InputManagement.refuelKey) && currentLocationCanFuel)
		{
			BuyFuel();
		}
	}
}
