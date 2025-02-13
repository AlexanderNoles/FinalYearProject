using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NationSelectionInteraction : Interaction
{
	public override bool ValidateOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		return target.simulationEntity != null;
	}

	public override void ProcessOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		//Should maybe display a "are you sure?" ui here first
		//but for now lets just create the faction

		PlayerManagement.InitPlayerFaction();

		//Set to first set
		SimulationEntity simEntity = target.simulationEntity;
		RealSpacePosition rps = null;

		if (simEntity.GetData(DataTags.Settlements, out SettlementsData setData) && setData.settlements.Count > 0)
		{
			SettlementsData.Settlement actualSet = setData.settlements.ElementAt(0).Value;
			rps = actualSet.actualSettlementPos.Clone();
			rps.Add(Vector3.back * (actualSet.location.GetEntryOffset() * WorldManagement.invertedInEngineWorldScaleMultiplier));
		}
		else if (simEntity.GetData(DataTags.Territory, out TerritoryData terrData) && terrData.territoryCenters.Count > 0) 
		{
			rps = terrData.territoryCenters.ElementAt(0).Clone();
		}

		if (rps != null)
		{
			//If the above check fails and no location is set then the player location management will auto find a random location
			PlayerLocationManagement.SetInitalLocationExternal(rps);
		}

		//Close map ui
		MapManagement.ClosePostNationSelection();
	}

	public override InteractionMapCursor GetMapCursorData()
	{
		return basicBorder;
	}

	public override bool OverrideUIState()
	{
		//Display regradless of neutral being active
		return true;
	}
}
