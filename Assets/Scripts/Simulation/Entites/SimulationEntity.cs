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
public class SimulationEntity : SimObject
{
    private static int currentNextID = 0;
    public int id = -1;
    private EntityLink link;
	public int createdTick;
	public string createdYear;

    public virtual void Simulate()
    {
        id = currentNextID;
        currentNextID++;

		createdTick = SimulationManagement.currentTickID;
		createdYear = SimulationManagement.GetCurrentYear().ToString();

		SimulationManagement.RegisterEntityToIDDict(this);

        link = new EntityLink(this);

        //Tags and Data are fundamental systems to describe simulation elements
        //
        //Data is split into modules that can be accessed that describe the actual makeup of an entity
        //Tags are used to generally describe a entity, they are used by routines to correctly access the correct entities to act on
        InitEntityTags();
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

    public virtual void InitEntityTags()
    {
        tags = new HashSet<Enum>();
    }

    //Data system
    //Data modules are stored with an accesor key, typically a tag
    //these accessor keys are used by routines to get specific sets of data modules
    protected Dictionary<Enum, DataModule> dataModules;

    public List<Enum> GetDataTags()
    {
        return dataModules.Keys.ToList();
    }

    public virtual void InitData()
    {
        //Add nothing by default
        dataModules = new Dictionary<Enum, DataModule>();
    }

	public bool HasData(Enum tag)
	{
		return dataModules.ContainsKey(tag);
	}

    //Get a data module
    public bool GetData<T>(Enum tag, out T data) where T : DataModule
    {
        bool toReturn = dataModules.TryGetValue(tag, out DataModule tempData);
        data = tempData as T;

        return toReturn;
    }

	public T GetDataDirect<T>(Enum tag) where T : DataModule
	{
		if (HasData(tag))
		{
			return dataModules[tag] as T;
		}

		return null;
	}

    //Add a data module
    public void AddData(Enum tag, DataModule data)
    {
        if (dataModules.ContainsKey(tag))
        {
            return;
        }

        //Set data parent so it can access it later
        data.parent = link;

		data.OnAdd();
        dataModules.Add(tag, data);

        //Add to simulation manamgement register, so routines can access this data module
        SimulationManagement.RegisterDataModule(tag, data);
    }

	public void ReplaceData(Enum tag, DataModule data)
	{
		if (dataModules.ContainsKey(tag))
		{
			RemoveData(tag);
			AddData(tag, data);
		}
	}

    public void RemoveData(Enum tag)
    {
        DataModule data = dataModules[tag];

		data.OnRemove();
		dataModules.Remove(tag);

        //Remove from simulation management register so routines don't pick it up
        SimulationManagement.DeRegisterDataModule(tag, data);
    }

	public List<DataModule> GetAllDataModules()
	{
		return dataModules.Values.ToList();
	}

	//Helper method
	public override Shop GetShop()
	{
		if (!HasData(DataTags.Economic))
		{
			return null;
		}

		GetData(DataTags.Economic, out EconomyData data);

		return data.market;
	}
}
