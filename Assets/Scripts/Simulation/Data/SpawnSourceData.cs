
using EntityAndDataDescriptor;
using System;

public class SpawnSourceData : DataModule
{
	public int source = -1;

	public virtual void OnSpawn(int source)
	{
		this.source = source;
	}

	public virtual EntityTypeTags GetSourceType()
	{
		if (SimulationManagement.EntityExists(source))
		{
			SimulationEntity sourceEntity = SimulationManagement.GetEntityByID(source);

			//Get the first entity type tag
			foreach (Enum tag in sourceEntity.GetEntityTags())
			{
				if (tag.GetType().Equals(typeof(EntityTypeTags)))
				{
					return (EntityTypeTags)tag;
				}
			}
		}

		return EntityTypeTags.Faction;
	}
}