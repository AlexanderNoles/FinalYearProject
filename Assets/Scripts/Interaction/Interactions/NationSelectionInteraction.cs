using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NationSelectionInteraction : Interaction
{
	public override bool ValidateOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		return target.simulationEntity != null && target.simulationEntity.HasData(DataTags.Name);
	}

	public override void ProcessOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		NationInformationDisplayUI.SetActive(!NationInformationDisplayUI.IsTarget(target.simulationEntity), target.simulationEntity);
	}

	public static void FinalizeNationSelect(SimulationEntity entity)
	{
		PlayerManagement.InitPlayerFaction();

		//Set to first set
		RealSpacePosition rps = null;

		if (entity.GetData(DataTags.Settlements, out SettlementsData setData) && setData.settlements.Count > 0)
		{
			SettlementsData.Settlement actualSet = setData.settlements.ElementAt(0).Value;
			rps = actualSet.actualSettlementPos.Clone();
			rps.Add(Vector3.back * (actualSet.location.GetEntryOffset() * WorldManagement.invertedInEngineWorldScaleMultiplier));
		}
		else if (entity.GetData(DataTags.TargetableLocation, out TargetableLocationData targetableLocationData))
		{
			rps = targetableLocationData.actualPosition.Clone();
			rps.Add(Vector3.back * (targetableLocationData.GetEntryOffset() * WorldManagement.invertedInEngineWorldScaleMultiplier));
		}
		else if (entity.GetData(DataTags.Territory, out TerritoryData terrData) && terrData.territoryCenters.Count > 0)
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
