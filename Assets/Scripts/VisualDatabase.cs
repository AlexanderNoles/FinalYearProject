using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VisualDatabase : MonoBehaviour
{
	public static readonly string goodColourString = "#4bed32ff";
	public static readonly string badColourString = "#c4372dff";
	public static readonly string statisticColour = "#d1b536";

    private static VisualDatabase instance;

    private static int currentColourIndex = 0;
    [Header("Emblem Data")]
    [Header("Colour")]
    public List<Color> factionColours = new List<Color>();

    [Header("Icons")]
    public string factionsIconsPath = "Assets/Textures/ui/Icons/NationIconSheet.png";
    public List<Sprite> factionIcons = new List<Sprite>();
	public string statIconsPath = "Assets/Textures/ui/Icons/StatIconSheet.png";
	public List<Sprite> statIcons = new List<Sprite>();

    private void Awake()
    {
        instance = this;
        currentColourIndex = Random.Range(0, factionColours.Count);
    }

    public static Color GetNextFactionColour()
    {
        currentColourIndex = (currentColourIndex + 1) % instance.factionColours.Count;

        return instance.factionColours[currentColourIndex];
    }

    public static (Sprite, Sprite) GetFactionIcons()
    {
        int mainIconIndex = SimulationManagement.random.Next(0, instance.factionIcons.Count);
        int backingIconIndex = SimulationManagement.random.Next(0, instance.factionIcons.Count);

        return (instance.factionIcons[mainIconIndex], instance.factionIcons[backingIconIndex]);
    }
	
	public static bool LoadIconFromResources(string name, out Sprite iconSprite)
	{
		//Load sprite from resources
		iconSprite = Resources.Load<Sprite>($"icons/{name}");

		return iconSprite != null;
	}

	public static Sprite GetStatSprite(Stats stat)
	{
		int index = (int)stat;

		if (index >= instance.statIcons.Count)
		{
			return null;
		}

		return instance.statIcons[index];
	}

#if UNITY_EDITOR
    [ContextMenu("Load Faction Icon Images")]
    public void LoadFactionIconImages()
    {
        factionIcons.Clear();
        Object[] data = AssetDatabase.LoadAllAssetsAtPath(factionsIconsPath);

        foreach (Object obj in data)
        {
            if (obj is Sprite)
            {
                factionIcons.Add(obj as Sprite);
            }
        }
    }

	[ContextMenu("Load Stat Icon Images")]
	public void LoadStatImages()
	{
		statIcons.Clear();
		Object[] data = AssetDatabase.LoadAllAssetsAtPath(statIconsPath);

		foreach (Object obj in data)
		{
			if (obj is Sprite)
			{
				statIcons.Add(obj as Sprite);
			}
		}
	}
#endif
}
