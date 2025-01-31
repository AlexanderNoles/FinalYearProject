using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PolicySlotControl : MonoBehaviour
{
	[Header("References")]
	public TextMeshProUGUI policyLabel;
	public DisplayOnHover displayOnHover;
	public GameObject tickBox;
	public Image tickBoxImage;
	public GameObject tickImage;

	private Policy targetPolicy;
	private PoliciesData targetData;
	private PolicyUIManagement parent;
	[HideInInspector]
	public bool policyActive;

	public void SetTarget(Policy policy, PoliciesData policiesData, PolicyUIManagement parent)
	{
		//Setup targeting
		targetPolicy = policy;
		displayOnHover.Setup(policy);

		targetData = policiesData;
		this.parent = parent;

		//Do ui alteration that only needs to be performed once

		//Set text
		policyLabel.text = targetPolicy.GetTitle();
	}

	public void InitialDraw()
	{
		//Is this policy active in the current state of the player's data when this ui is opened?
		SetPolicyActiveUI(targetData.activePolicies.Contains(targetPolicy));
	}

	//Used as a button onclick callback
	public void TogglePolicy()
	{
		SetPolicyActiveUI(!policyActive);
	}

	//Just sets visual ui, changes to fundamental data are applied on apply button pressed
	public void SetPolicyActiveUI(bool _bool)
	{
		policyActive = _bool;
		tickImage.SetActive(policyActive);
		//
		parent.NotifyOfPolicyState(policyActive, targetPolicy);

		if (policyActive)
		{
			tickBoxImage.color = Color.black;
		}
		else
		{
			tickBoxImage.color = Color.white;
		}
	}

	public void OnPoliciesStateChange(bool atMaxPolicies)
	{
		//Hide tickbox if not active and at max capacity
		tickBox.SetActive(!(atMaxPolicies && !policyActive));
	}
}
