using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SimulationSettings : MonoBehaviour
{
    private static SimulationSettings instance;
    public bool runHistory;
    public bool drawSettlements;
    public bool updateMap;

    private void Awake()
    {
        instance = this;
    }

    public static bool ShouldRunHistory()
    {
#if UNITY_EDITOR
        return instance.runHistory;
#endif
        return true;
    }

    public static bool DrawSettlements()
    {
#if UNITY_EDITOR
        return instance.drawSettlements;
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

    [MenuItem("PlaySettings/Flip Run History")]
    public static void FlipRunHistory()
    {
        SimulationSettings preRunManagement = FindAnyObjectByType<SimulationSettings>();

        preRunManagement.runHistory = !preRunManagement.runHistory;
        Debug.Log($"Run History: {preRunManagement.runHistory}");
    }

    [MenuItem("PlaySettings/Draw Settlements")]
    public static void FlipDrawSettlements()
    {
        SimulationSettings preRunManagement = FindAnyObjectByType<SimulationSettings>();

        preRunManagement.drawSettlements = !preRunManagement.drawSettlements;
        Debug.Log($"Draw Settlements: {preRunManagement.drawSettlements}");
    }

    [MenuItem("PlaySettings/Update Map")]
    public static void FlipRedrawMap()
    {
        SimulationSettings preRunManagement = FindAnyObjectByType<SimulationSettings>();

        preRunManagement.updateMap = !preRunManagement.updateMap;
        Debug.Log($"Update Map: {preRunManagement.updateMap}");
    }
}
