using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettlementData : DataBase
{
    public class Settlement
    {

    }

    public Dictionary<RealSpacePostion, Settlement> settlements = new Dictionary<RealSpacePostion, Settlement>();
    public int settlementCapacity = 5;
}
