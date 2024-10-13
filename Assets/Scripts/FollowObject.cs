using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public Transform target;

    public enum Mode
    {
        Exact,
        Lerp,
        Constant
    }

    public Mode mode;

    [Header("Lerp")]
    public float lerpSpeed;
    public float lerpLimit = 0.01f;

    [Header("Constant")]
    public float maxMove = 1.0f;

    [Header("Offset")]
    public Vector3 offset;

    public enum OffsetMode
    {
        Relative,
        Absolute
    }

    public OffsetMode offsetMode;

    private void LateUpdate()
    {
        Vector3 targetPos = target.position;

        if (offsetMode == OffsetMode.Relative)
        {
            targetPos += offset;
        }
        else
        {
            Vector3 mask = offset;
            mask.x = mask.x != 0 ? 1 : 0;
            mask.y = mask.y != 0 ? 1 : 0;
            mask.z = mask.z != 0 ? 1 : 0;

            targetPos = new Vector3(
                Mathf.Lerp(targetPos.x, offset.x, mask.x),
                Mathf.Lerp(targetPos.y, offset.y, mask.y),
                Mathf.Lerp(targetPos.z, offset.z, mask.z)
                );
        }

        float distance = Vector3.Distance(targetPos, transform.position);

        if ((mode == Mode.Lerp && distance < lerpLimit) || mode == Mode.Exact)
        {
            transform.position = targetPos;
        }
        else if (mode == Mode.Lerp)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * lerpSpeed);
        }
        else if (mode == Mode.Constant)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * maxMove);
        }
    }
}
