using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualDatabase : MonoBehaviour
{
    private static VisualDatabase instance;

    private static int currentColourIndex = 0;
    public List<Color> factionColours = new List<Color>();

    private static int currentIconIndex = 0;
    public List<Sprite> factionIcons = new List<Sprite>();

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
}
