using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityLink
{
    //Class used to allow data modules to access their parent entity
    public SimulationEntity target;

    public SimulationEntity Get()
    {
        return target;
    }

    public EntityLink(SimulationEntity target)
    {
        this.target = target;
    }
}
