using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationData : DataModule
{
	public bool variablePopulation = true;

    public float currentPopulationCount;
    public float populationNaturalGrowthSpeed;
    public float populationNaturalDeathSpeed;
    public float populationNaturalGrowthLimt;
}
