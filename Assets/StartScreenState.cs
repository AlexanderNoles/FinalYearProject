using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartScreenState : UIState
{
	[HideInInspector]
	public EmblemData emblemData;
	public EmblemRenderer emblemRenderer;

	public override KeyCode GetSetActiveKey()
	{
		return KeyCode.Return;
	}

	private void Start()
	{
		//Init
		emblemData = new EmblemData();
		//Mark as processed
		emblemData.hasCreatedEmblem = true;

		SetMainColour(VisualDatabase.GetNextFactionColour());
		SetMainIcon(VisualDatabase.GetFactionIconNonDeterministic());
		SetBackingIcon(VisualDatabase.GetFactionIconNonDeterministic());
	}

	public void DrawEmblem()
	{
		emblemRenderer.Draw(emblemData);
	}

	public void SetMainColour(Color color)
	{
		emblemData.mainColour = color;
		emblemData.SetColoursBasedOnMainColour();

		DrawEmblem();
	}

	public void SetMainIcon(Sprite icon)
	{
		emblemData.mainIcon = icon;

		DrawEmblem();
	}

	public void SetBackingIcon(Sprite icon)
	{
		emblemData.backingIcon = icon;

		DrawEmblem();
	}
}
