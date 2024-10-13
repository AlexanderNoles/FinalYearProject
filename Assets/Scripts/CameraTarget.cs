using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public int priority = 10;

    private void Start()
    {
        CameraManagement.AddCameraTarget(priority, transform);
    }
}
