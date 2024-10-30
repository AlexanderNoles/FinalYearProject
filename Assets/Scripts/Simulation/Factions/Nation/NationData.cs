using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NationData : FactionData
{
    //Define tags

    public HashSet<Tags> tagList = new HashSet<Tags>()
    {
        Tags.Nation
    };

    public override bool HasTag(Tags tag)
    {
        return tagList.Contains(tag);
    }

    //
}
