using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationDisplayControl : MonoBehaviour
{
	public GameObject blocker;

	[Header("General")]
	public TextMeshProUGUI title;
	public Image backingImage;
	public TextMeshProUGUI description;

	private string normalDescriptionString;
	private string extendedDescriptionString;

	private bool showingExtendedDescription;
	private bool noExtraInfo;

	public void Draw(IDisplay input)
	{
		string descriptionText = input.GetDescription();
		string extraInformation = input.GetExtraInformation();

		showingExtendedDescription = false;
		normalDescriptionString = descriptionText;

		noExtraInfo = string.IsNullOrEmpty(extraInformation);
		if (noExtraInfo)
		{
			normalDescriptionString += "\n\n\n<i><color=#696969>No extra information...</color></i>";
			extendedDescriptionString = "";
		}
		else
		{
			string additionalString = "\n\n\n<i><color=#696969>Press T to toggle extra information...</color></i>";

			normalDescriptionString += additionalString;
			extendedDescriptionString = descriptionText + "\n\n<i><color=#696969>" + extraInformation + "</color></i>" + additionalString;
		}

		title.text = input.GetTitle();
		backingImage.sprite = input.GetBackingImage();
		description.text = normalDescriptionString;

		blocker.SetActive(false);
	}

	private void OnEnable()
	{
		DisplayBlocker();
	}

	public void DisplayBlocker()
	{
		blocker.SetActive(true);
	}

	private void Update()
	{
		if (!blocker.activeSelf && InputManagement.GetKeyDown(KeyCode.T) && !string.IsNullOrEmpty(extendedDescriptionString))
		{
			if (showingExtendedDescription)
			{
				description.text = normalDescriptionString;
			}
			else
			{
				description.text = extendedDescriptionString;
			}

			showingExtendedDescription = !showingExtendedDescription;
		}
	}
}
