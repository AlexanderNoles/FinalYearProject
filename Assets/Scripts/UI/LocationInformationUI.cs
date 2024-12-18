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
	public ShopControlUI shopControl;
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

		if (shopControl.IsDisplayedData(shopData))
		{
			shopControl.Hide();
		}
	}

	public void Draw()
	{
		if (cachedLocation != null && shopControl.IsDisplayedData(shopData))
		{
			//Don't want to be able to access shop when we have left location
			shopControl.Hide();
		}

		//Get current location
		VisitableLocation currentLocation = PlayerLocationManagement.GetCurrentPrimaryLocation();
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

	public void ToggleShopButton()
	{
		shopControl.ToggleOrGrab(shopData);
		shopWindow.MoveToFront();
	}

	public void BuyFuel()
	{
		//Get player inventory
		List<Faction> players = SimulationManagement.GetAllFactionsWithTag(Faction.Tags.Player);

		if(players.Count > 0)
		{
			players[0].GetData(PlayerFaction.inventoryDataKey, out PlayerInventory playerInventory);

			const float maxFuelBuyRate = 50;
			float moneyToSpend = Mathf.Min(maxFuelBuyRate, playerInventory.mainCurrency);

			if (moneyToSpend > 0)
			{
				//Can afford any fuel
				//Subtract money
				playerInventory.mainCurrency -= moneyToSpend;
				playerInventory.fuel += moneyToSpend * cachedFuelPerMoneyUnit;

				//Redraw main bar
				MainInfoBarControl.ForceRedraw();
			}
		}
	}

	private void Update()
	{
		if (blocker.activeSelf)
		{
			return;
		}

		if (InputManagement.GetKeyDown(KeyCode.Q) && cachedLocation.HasShop())
		{
			//Toggle shop
			ToggleShopButton();
		}

		//If jumping don't allow refuel
		//This is because it interfers with the fuel count down during jump effect
		//Also it would be weird to buy fuel will about to jump
		fuelButton.Enable(currentLocationCanFuel && !PlayerCapitalShip.IsJumping());
	}
}
