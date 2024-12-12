using System.Collections;
using System.Collections.Generic;
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
		//Get current location
		VisitableLocation currentLocation = PlayerLocationManagement.GetCurrentLocation();
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
		}
	}

	public void ToggleShopButton()
	{
		shopControl.ToggleOrGrab(shopData);
	}

	private void Update()
	{
		if (InputManagement.GetKeyDown(KeyCode.Q) && cachedLocation.HasShop())
		{
			//Toggle shop
			ToggleShopButton();
		}
	}
}
