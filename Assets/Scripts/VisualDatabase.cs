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
    public string iconsPath = "Assets/Textures/ui/Icons/NationIconSheet.png";
    public List<Sprite> factionIcons = new List<Sprite>();

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

#if UNITY_EDITOR
    [ContextMenu("Load Icon Images")]
    public void LoadIconImages()
    {
        factionIcons.Clear();
        Object[] data = AssetDatabase.LoadAllAssetsAtPath(iconsPath);

        foreach (Object obj in data)
        {
            if (obj is Sprite)
            {
                factionIcons.Add(obj as Sprite);
            }
        }
    }
#endif
}
