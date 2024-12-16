using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SimulationSettings : MonoBehaviour
{
    private static SimulationSettings instance;
	public int historyLength = 30;
	public bool drawMilitaryPresence;
    public bool updateMap;

    private void Awake()
    {
        instance = this;
    }

    public static bool ShouldRunHistory()
    {
		return true;
	}

	public static int HistoryLength()
	{
		return instance.historyLength;
	}

	public static bool DrawMilitaryPresence()
	{
#if UNITY_EDITOR
		return instance.drawMilitaryPresence;
#endif
		return true;
	}


	public static bool UpdateMap()
    {
#if UNITY_EDITOR
        return instance.updateMap;
#endif
        return true;
    }
}
