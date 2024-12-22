using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionLink
{
    //Class used to allow data modules to access their parent faction
    public Faction target;

    public Faction Get()
    {
        return target;
    }

    public FactionLink(Faction target)
    {
        this.target = target;
    }
}
