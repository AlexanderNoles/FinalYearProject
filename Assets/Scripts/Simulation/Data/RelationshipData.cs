using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipData : DataBase
{
    public class Relationship
    {
        //This is what this faction thinks about the other faction

        public float favourability;

        public Relationship(float baseFavour)
        {
            favourability = baseFavour;
        }
    }

    public Dictionary<int, Relationship> idToRelationship = new Dictionary<int, Relationship>();
    public float baseFavourability = 0.5f; //What other factions inherently think of this one (For a murderous swarm for example this should be zero)
}
