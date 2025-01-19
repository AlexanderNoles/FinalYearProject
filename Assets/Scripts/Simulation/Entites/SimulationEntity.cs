using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityAndDataDescriptor;
using System;
using System.Linq;

/// <summary>
/// The fundamental entity script, describes only the most basic information
/// Includes systems for expansion to better describe an entity
/// </summary>
public class SimulationEntity
{
    private static int currentNextID = 0;
    public int id = -1;
    private EntityLink link;

    public virtual void Simulate()
    {
        id = currentNextID;
        currentNextID++;

        SimulationManagement.RegisterEntityToIDDict(this);

        link = new EntityLink(this);

        //Tags and Data are fundamental systems to describe simulation elements
        //
        //Data is split into modules that can be accessed that describe the actual makeup of an entity
        //Tags are used to generally describe a entity, they are used by routines to correctly access the correct entities to act on
        InitTags();
        InitData();
    }

    private HashSet<Enum> tags;

    public HashSet<Enum> GetEntityTags()
    {
        return tags;
    }

    public bool HasTag(Enum tag)
    {
        return tags.Contains(tag);
    }

    public void AddTag(Enum tag)
    {
        //Add to simulation manager tag find system
        SimulationManagement.RegisterEntityHasTag(tag, this);
        //Add to personal tags
        tags.Add(tag);
    }

    public void RemoveTag(Enum tag)
    {
        //Remove from simulation manager tag find system
        SimulationManagement.DeRegisterEntityHasTag(tag, this);
        //Remove from personal tags
        tags.Remove(tag);
    }

    public virtual void InitTags()
    {
        tags = new HashSet<Enum>();
    }

    //Data system
    //Data modules are stored with an accesor key, typically a tag
    //these accessor keys are used by routines to get specific sets of data modules
    protected Dictionary<Enum, DataBase> dataModules;

    public List<Enum> GetDataTags()
    {
        return dataModules.Keys.ToList();
    }

    public virtual void InitData()
    {
        //Add nothing by default
        dataModules = new Dictionary<Enum, DataBase>();
    }

	public bool HasData(Enum tag)
	{
		return dataModules.ContainsKey(tag);
	}

    //Get a data module
    public bool GetData<T>(Enum tag, out T data) where T : DataBase
    {
        bool toReturn = dataModules.TryGetValue(tag, out DataBase tempData);
        data = tempData as T;

        return toReturn;
    }

    //Add a data module
    public void AddData(Enum tag, DataBase data)
    {
        if (dataModules.ContainsKey(tag))
        {
            return;
        }

        //Set data parent so it can access it later
        data.parent = link;

        dataModules.Add(tag, data);

        //Add to simulation manamgement register, so routines can access this data module
        SimulationManagement.RegisterDataModule(tag, data);
    }

    public void RemoveData(Enum tag)
    {
        DataBase data = dataModules[tag];
        dataModules.Remove(tag);

        //Remove from simulation management register so routines don't pick it up
        SimulationManagement.DeRegisterDataModule(tag, data);
    }



	public virtual void OnDeath()
	{
		//Do nothing by default
	}
}
