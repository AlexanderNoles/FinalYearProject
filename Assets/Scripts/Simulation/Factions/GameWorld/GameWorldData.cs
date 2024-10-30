using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWorldData : FactionData
{
    //Define tags

    public HashSet<Tags> tagList = new HashSet<Tags>()
    {
        Tags.GameWorld
    };

    public override bool HasTag(Tags tag)
    {
        return tagList.Contains(tag);
    }

    //
}
