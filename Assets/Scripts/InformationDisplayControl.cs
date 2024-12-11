using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InformationDisplayControl : MonoBehaviour
{
	public GameObject blocker;
	public bool lockBlocker = false;

	[Header("General")]
	public TextMeshProUGUI title;
	public TextMeshProUGUI description;
	public GameObject flash;

	private string normalDescriptionString;
	private string extendedDescriptionString;

	private bool showingExtendedDescription;
	private bool noExtraInfo;

	public void Draw(IDisplay input)
	{
		//Flash
		flash.SetActive(false);
		flash.SetActive(true);
		//

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
		description.text = normalDescriptionString;

		SetBlocker(false);
	}

	private void OnEnable()
	{
		DisplayBlocker();
	}

	public void DisplayBlocker()
	{
		SetBlocker(true);
	}

	private void SetBlocker(bool _bool)
	{
		if (lockBlocker)
		{
			return;
		}

		blocker.SetActive(_bool);
	}

	private void Update()
	{
		if (!blocker.activeSelf && InputManagement.GetKeyDown(KeyCode.T) && !noExtraInfo)
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
