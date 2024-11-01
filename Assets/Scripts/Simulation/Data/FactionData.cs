using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionData : DataBase
{
    public bool deathFlag = false;

    public void ForceDeath()
    {
        deathFlag = true;
    }
}
