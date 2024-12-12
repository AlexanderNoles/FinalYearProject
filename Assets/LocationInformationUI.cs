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
	private FloatingWindow shopWindow;

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
		shopWindow.MoveToFront();
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
