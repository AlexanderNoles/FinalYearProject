
using EntityAndDataDescriptor;
using System;
using System.Collections.Generic;

public class EntitySpawnData : DataModule
{
	public class EntityToSpawn
	{
		public Type entityClassType = typeof(MineralDeposit);
		public EntityTypeTags entityTag;
		public int countPer = 1;
		public int monthFrequency = 1;
		public int chance = 1;
		public float tendencyTowardsCap = 0;
		public int totalMax = -1;
	}

	public List<EntityToSpawn> targets = new List<EntityToSpawn>();
}