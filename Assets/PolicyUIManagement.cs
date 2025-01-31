using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;
using UnityEngine.Events;
using UnityEngine.Assertions;

public class PolicyUIManagement : UIState
{
	//Generate a policy control slot for each possible policy
	//They need to be created as instances so we can call their IDisplay methods, it also means we can do all the object instantiation upfront
	//We then keep a reference to each slot (as a PolicySlotControl) and handle selection interaction through that 

	//Then when apply button is pressed we apply those results to the player's policy data by scanning all the policy control slots and adding any selected policies
	//We also need to keep track of the number of ticked policies so we can limit the player to an allowed number

	private UnityEvent<bool> onPolicyStateChange = new UnityEvent<bool>();
	private HashSet<Policy> selectedPolicies = new HashSet<Policy>();
	private List<PolicySlotControl> policySlots = new List<PolicySlotControl>();
	[HideInInspector]
	public int activePoliciesCount = 0;

	[Header("Settings")]
	public int numberOfAllowedPolicies = 3;

	[Header("Policy Display")]
	public GameObject target;
	public GameObject baseSlot;
	public RectTransform slotArea;

	public override KeyCode GetSetActiveKey()
	{
		return InputManagement.togglePolicyKey;
	}

	protected override GameObject GetTargetObject()
	{
		return target;
	}

	protected override void OnSetActive(bool _bool)
	{
		if (_bool)
		{
			//No slots drawn and player entity exists
			if (policySlots.Count == 0 && PlayerManagement.PlayerEntityExists())
			{
				selectedPolicies.Clear();
				//Player policies data
				PlayerManagement.GetTarget().GetData(DataTags.Policies, out PoliciesData policiesData);

				//If all policies hasn't been initalized yet this loop won't run and no entries will be added to policySlots
				foreach (Type policy in PoliciesData.allPolicies)
				{
					//Create new slot
					PolicySlotControl newSlot = Instantiate(baseSlot, slotArea).GetComponent<PolicySlotControl>();
					RectTransform newSlotRect = newSlot.transform as RectTransform;

					//Set position
					newSlotRect.anchoredPosition3D = new Vector3(0, -100 - (50 * policySlots.Count), 0.0f);

					//Create new policy object and pass it to the new slot
					newSlot.SetTarget(Activator.CreateInstance(policy) as Policy, policiesData, this);

					policySlots.Add(newSlot);
					onPolicyStateChange.AddListener(newSlot.OnPoliciesStateChange);
				}
			}

			//Do inital draw
			//This is done if any slots exist at all as we want to reset to reflect the current active policies on enable
			//instead of maintaing any unapplied changes when this state was set inactive
			if (policySlots.Count > 0)
			{
				foreach (PolicySlotControl slotControl in policySlots)
				{
					slotControl.InitialDraw();
				}
			}
		}
	}

	public void NotifyOfPolicyState(bool active, Policy target)
	{
		if (active)
		{
			//Add to selected policies
			if (!selectedPolicies.Contains(target) && selectedPolicies.Count < numberOfAllowedPolicies)
			{
				selectedPolicies.Add(target);
			}
		}
		else
		{
			//Remove from selected policies
			if (selectedPolicies.Contains(target))
			{
				selectedPolicies.Remove(target);
			}
		}

		onPolicyStateChange.Invoke(selectedPolicies.Count >= numberOfAllowedPolicies);
	}

	public void ApplySelectedPolicies()
	{
		//Copy select policies over to the actual player data
		if (PlayerManagement.PlayerEntityExists())
		{
			Assert.IsTrue(PlayerManagement.GetTarget().GetData(DataTags.Policies, out PoliciesData target));

			target.activePolicies = selectedPolicies;
		}

		//Close the policies ui automatically
		UIManagement.ReturnToNeutral();
	}
}

