using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCenter : MonoBehaviour
{
    Vector3 positionLastFrame;

    private void Awake()
    {
        positionLastFrame = transform.position;

        WorldManagement.SetWorldCenterPosition(new RealSpacePosition(positionLastFrame.x, positionLastFrame.y, positionLastFrame.z));
    }

    private void Update()
    {
        Vector3 offset = transform.position - positionLastFrame;

        WorldManagement.MoveWorldCenter(offset);

        positionLastFrame = transform.position;
    }
}
