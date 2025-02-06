using System;
using System.Collections;
using System.Collections.Generic;
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
}
