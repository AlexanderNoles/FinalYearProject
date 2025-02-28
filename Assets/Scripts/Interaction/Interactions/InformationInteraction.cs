using EntityAndDataDescriptor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InformationInteraction : Interaction
{
	public override bool ValidateOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		return target.baseLocation == null && target.simulationEntity != null && target.simulationEntity.HasData(DataTags.Name) && !PlayerCapitalShip.IsJumping();
	}

	public override void ProcessOnMap(PlayerMapInteraction.UnderMouseData target)
	{
		DisplayEntityInformation(target.simulationEntity);
	}

	public override bool ValidateBehaviour(SimObjectBehaviour interactable)
	{
		return
			interactable.target != null &&
			interactable.target.parent != null &&
			interactable.target.parent.Get().HasData(DataTags.Name);
	}

	public override void ProcessBehaviour(SimObjectBehaviour interactable)
	{
		DisplayEntityInformation(interactable.target.parent.Get());
	}

	private void DisplayEntityInformation(SimulationEntity target)
	{
		EntityInformationDisplay.ToggleExternal(target);
	}

	protected override string GetIconPath()
	{
		return "informationInteraction";
	}

	public override int GetDrawPriority()
	{
		return 65;
	}

	public override InteractionMapCursor GetMapCursorData(PlayerMapInteraction.UnderMouseData target)
	{
		return basicBorder;
	}

	public override string GetTitle()
	{
		return "Info";
	}
}
