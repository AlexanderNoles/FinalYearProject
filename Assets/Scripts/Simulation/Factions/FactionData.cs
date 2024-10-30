using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionData
{
    public enum Tags
    {
        Nation,
        GameWorld
    }

    public virtual bool HasTag(Tags tag)
    {
        return false;
    }
}
