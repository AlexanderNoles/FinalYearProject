using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPosition : MonoBehaviour
{
    public bool active = true;

    public enum LookType
    {
        DynamicPlayer,
        PlayerAtEyeLevel,
        PlayerEyeLevelWhenClose,
        StaticWorld
    }

    public LookType lookType;

    [Header("Eye Level When Close")]
    public float startDistance = 1;
    public float endDistance = 0.1f;

    [Header("Static World")]
    public Vector3 staticWorldPos = Vector3.zero;

    [Header("Lerp")]
    public bool lerp = false;
    public float lerpSpeed = 1.0f;

    private Vector3 targetPos;
    [Header("Extra")]
    public Vector3 additionalRotation = Vector3.zero;

    private void Update()
    {
        if (!active)
        {
            return;
        }


        Vector3 pos = staticWorldPos;

        if (lookType != LookType.StaticWorld)
        {
            pos = PlayerManagement.GetPosition();
            if (lookType == LookType.PlayerAtEyeLevel)
            {
                pos.y = transform.position.y;
            }
            else if (lookType == LookType.PlayerEyeLevelWhenClose)
            {
                float distancePercentage = (Vector3.Distance(pos, transform.position) - endDistance) / (startDistance - endDistance);

                pos.y = Mathf.Lerp(transform.position.y, pos.y, distancePercentage);
            }
        }

        if (lerp)
        {
            targetPos = Vector3.Lerp(targetPos, pos, Time.deltaTime * lerpSpeed);
        }
        else
        {
            targetPos = pos;
        }

        transform.LookAt(targetPos);
        transform.Rotate(additionalRotation, Space.Self);
    }
}
