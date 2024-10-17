using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoutineBase : MonoBehaviour
{
    public virtual void Run(Faction faction)
    {

    }

    public virtual bool Check(Faction faction)
    {
        return true;
    }
}
