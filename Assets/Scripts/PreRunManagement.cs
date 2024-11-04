using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class PreRunManagement : MonoBehaviour
{
    private static PreRunManagement instance;
    public bool runHistory;

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

    [MenuItem("PlaySettings/FlipRunHistory")]
    public static void FlipRunHistory()
    {
        PreRunManagement preRunManagement = FindAnyObjectByType<PreRunManagement>();

        preRunManagement.runHistory = !preRunManagement.runHistory;
        Debug.Log($"Run History: {preRunManagement.runHistory}");
    }
}
