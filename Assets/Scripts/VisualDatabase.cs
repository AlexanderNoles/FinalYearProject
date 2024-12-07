using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualDatabase : MonoBehaviour
{
	public static readonly string goodColourString = "#4bed32ff";
	public static readonly string badColourString = "#c4372dff";
	public static readonly string statisticColour = "#d1b536";




	[Header("Other Factions")]
    private static VisualDatabase instance;

    private static int currentColourIndex = 0;
    public List<Color> factionColours = new List<Color>();

    private static int currentIconIndex = 0;
    public List<Sprite> factionIcons = new List<Sprite>();

	[Header("Player")]
	public List<Sprite> unitIconImages = new List<Sprite>();

	private static readonly Dictionary<System.Type, int> unitTypeToIconSprite = new Dictionary<System.Type, int>() 
	{
		{typeof(LivingQuartersUnit), 0},
		{typeof(FactoryUnit), 1}
	};

    private void Awake()
    {
        instance = this;
        currentColourIndex = Random.Range(0, factionColours.Count);
        currentIconIndex = Random.Range(0, factionIcons.Count);
    }

    public static Color GetNextFactionColour()
    {
        currentColourIndex = (currentColourIndex + 1) % instance.factionColours.Count;

        return instance.factionColours[currentColourIndex];
    }

    public static Sprite GetNextFactionSprite()
    {
        currentIconIndex = (currentIconIndex + 1) % instance.factionIcons.Count;

        return instance.factionIcons[currentIconIndex];
    }

	public static Sprite GetUnitSpriteFromType(System.Type type)
	{
		if (instance != null && unitTypeToIconSprite.ContainsKey(type))
		{
			return instance.unitIconImages[unitTypeToIconSprite[type]];
		}

		return null;
	}
}
