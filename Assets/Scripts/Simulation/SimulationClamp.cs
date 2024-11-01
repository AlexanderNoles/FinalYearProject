using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationClamp : MonoBehaviour
{
    //As per the specification this runs before any of our other scripts

    void Update()
    {
        SimulationManagement.EndSimulationTick();
    }
}
