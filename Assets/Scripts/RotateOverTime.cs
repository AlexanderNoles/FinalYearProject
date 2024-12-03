using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    private new Transform transform;
    public float speed = 1.0f;
    public Vector3 rotation;
    public Space rotationSpace;
    public bool timeScaleIndependent = false;

    private void Awake()
    {
        transform = base.transform;
    }

    private void Update()
    {
        float timeScaler = Time.deltaTime;
        if (timeScaleIndependent)
        {
            timeScaler = Time.unscaledDeltaTime;
        }

        transform.Rotate(rotation * timeScaler * speed, rotationSpace);
    }
}
