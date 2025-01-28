using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A location is a sim object so that it can be drawn/represented by game loop side code
//Typically the visitable locations child class is used because it actually implements many of the functions automatically
public class Location : SimObject
{
    public virtual RealSpacePosition GetPosition()
    {
        throw new System.NotImplementedException();
    }
}
