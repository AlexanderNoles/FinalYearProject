using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitRoutineBase : RoutineBase
{
    public virtual bool IsDataToInit(HashSet<Enum> tags)
    {
        return false;
    }
}
