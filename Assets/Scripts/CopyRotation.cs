using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyRotation : MonoBehaviour
{
    public Transform target;
    private Transform localTransform;

    private void Awake()
    {
        localTransform = transform;
    }

    private void LateUpdate()
    {
        if (localTransform != null)
        {
            localTransform.rotation = target.rotation;
        }
    }

}
