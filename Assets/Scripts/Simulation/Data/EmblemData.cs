using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmblemData : DataModule
{
    public bool hasCreatedEmblem = false;
	public bool hasSetColours = false;
    public Color mainColour;
	public string mainColourHex;
    public Color highlightColour;
    public Color shadowColour;
    public Sprite mainIcon;
    public Sprite backingIcon;

	//Currently just randomizes the hue
	//When setting maximum change to true keep in mind that across two iterations the colour could end up back where it started
	public void SlightlyRandomize(float randomOffset = 0.05f, bool maximumChange = false)
	{
		Color.RGBToHSV(mainColour, out float baseH, out float baseS, out float baseV);

		//Loop them into 0...1
		float min = (baseH - randomOffset) % 1.0f;
		float max = (baseH + randomOffset) % 1.0f;

		if (min > max)
		{
			//Swap if min is greater than max
			(min, max) = (max, min);
		}

		//Lerp between min and max hue
		float lerpT;

		if (maximumChange)
		{
			lerpT = SimulationManagement.random.Next(0, 2);
		}
		else
		{
			const float res = 10000;
			lerpT = SimulationManagement.random.Next(0, 10000 + 1) / res;
		}

		mainColour = Color.HSVToRGB(Mathf.Lerp(min, max, lerpT), baseS, baseV);

		//Setup main colors
		SetColoursBasedOnMainColour();
	}

	public void SetColoursBasedOnMainColour()
	{
		mainColourHex = "#" + ColorUtility.ToHtmlStringRGB(mainColour);

		highlightColour = mainColour * 2.0f;
		shadowColour = mainColour * 0.25f;

		hasSetColours = true;
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
