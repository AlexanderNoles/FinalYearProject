using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipData : DataBase
{
    public class Relationship
    {
		//This is what this faction thinks about the other faction
		//How much they are in conflict with them
		//If this falls below a certain threshold for example then a war could end
		//This is seperated from favourability for a variety of reasons but you can think of it as just keeping the possibility space open
		public float conflict;
		//Generally how much they like them
		public float favourability;

        public Relationship(float baseFavour)
        {
            favourability = baseFavour;
        }
    }

    public Dictionary<int, Relationship> idToRelationship = new Dictionary<int, Relationship>();
    public float baseFavourability = 0.5f; //What other factions inherently think of this one (For a murderous swarm for example this should be zero)
}
