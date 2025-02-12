using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeelingsData : DataModule
{
    public class Relationship
    {
		//This is what this entity thinks about the target entity
		//Are they in conflict?
		//This can be set by a variety of things, perhaps could be a float instead that indicates how much they are in conflict with them?
		public bool inConflict;
		//Generally how much they like them
		//Used as a general benchmark by routines that don't care a huge amount about specifics
		//Also sets a base line
		public float favourability;
		//Instability of a relationship indicates how much it generally changes over time
		//Being in an alliance can lower instability for example
		public const float baseInstability = 0.025f;
		public float instability = baseInstability;

        public Relationship(float baseFavour)
        {
            favourability = baseFavour;
        }
    }

    public Dictionary<int, Relationship> idToFeelings = new Dictionary<int, Relationship>();
    public float baseFavourability = 0.5f; //What other entities inherently think of this one (For a murderous swarm for example this should be zero)
	public bool matching = false; //Auto match relationships? 

	public override string Read()
	{
		string output = "";

        foreach (KeyValuePair<int, Relationship> entry in idToFeelings)
        {
			output += $"	{entry.Key}: {entry.Value.favourability} ({entry.Value.inConflict})\n";
        }

        return output;
	}
}
