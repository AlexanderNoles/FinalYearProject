using EntityAndDataDescriptor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//DataBase is the base for all data
//It is used so different pieces of data can communicate with the simulation
//effectively.
public class DataModule
{
    public EntityLink parent;

    public bool TryGetLinkedData<T>(Enum tag, out T target) where T : DataModule
    {
        target = null;

        if (parent != null)
        {
            if (parent.Get().GetData(tag, out target))
            {
                return true;
            }
        }

        return false;
    }

    public T GetLinkedData<T>(Enum tag) where T : DataModule
    {
        parent.Get().GetData(tag, out T data);
        return data;
    }

	public virtual void OnAdd()
	{

	}

	public virtual void OnRemove()
	{

	}

	public bool ReadImplemented()
	{
		return Read() != null;
	}

	public virtual string Read()
	{
		return null;
	}

	//HELPERS

	//STATIC

	public static T ShallowCopy<T>(DataModule input) where T : DataModule
	{
		return (T)input.MemberwiseClone();
	}

	//NON STATIC

	public List<RealSpacePosition> GetSafetyPositions()
	{
		List<RealSpacePosition> toReturn = new List<RealSpacePosition>();

		if (TryGetLinkedData(DataTags.Settlements, out SettlementsData settlementsData))
		{
			foreach (SettlementsData.Settlement set in settlementsData.settlements.Values)
			{
				toReturn.Add(set.actualSettlementPos);
			}
		}

		if (TryGetLinkedData(DataTags.Refinery, out RefineryData refData))
		{
			if (refData.refineryPosition != null)
			{
				//Insert refinery at the beginning of the list so it takes preference
				toReturn.Insert(0, refData.refineryPosition);
			}
		}

		return toReturn;
	}
}
