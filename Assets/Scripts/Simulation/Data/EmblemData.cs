using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmblemData : DataModule
{
    public bool hasCreatedEmblem = false;
    public Color mainColour;
	public string mainColourHex;
    public Color highlightColour;
    public Color shadowColour;
    public Sprite mainIcon;
    public Sprite backingIcon;

	public void SetColoursBasedOnMainColour()
	{
		mainColour *= (1.0f + (SimulationManagement.random.Next(-100, 101) / 1000.0f));
		mainColourHex = "#" + ColorUtility.ToHtmlStringRGB(mainColour);

		highlightColour = mainColour * 2.0f;
		shadowColour = mainColour * 0.25f;
	}

	public override string Read()
	{
		return "	" +
			ColourString(highlightColour, "H") + " " +
			ColourString(mainColour, "M") + " " +
			ColourString(shadowColour, "S") + " "
			;
	}

	private string ColourString(Color color, string input)
	{
		return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>" + input + "</color>";
	}
}
