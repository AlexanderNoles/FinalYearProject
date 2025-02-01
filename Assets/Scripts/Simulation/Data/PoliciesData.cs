using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class PoliciesData : DataModule
{
	//Get all policies as type
	public static List<Type> allPolicies = new List<Type>();

	public HashSet<Policy> activePolicies = new HashSet<Policy>();

	public PoliciesData Init()
	{
		activePolicies.Clear();

		if (allPolicies.Count == 0)
		{
			//All policies not created
			//Get all policies
			allPolicies = new List<Type>();

			foreach (Type type in Assembly.GetAssembly(typeof(Policy)).GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(Policy))))
			{
				allPolicies.Add(type);
			}
		}

		return this;
	}
}

public class Policy : IDisplay
{
	public bool active = false;

	public virtual string GetDescription()
	{
		return "A policy";
	}

	public virtual string GetExtraInformation()
	{
		return "";
	}

	public virtual Sprite GetIcon()
	{
		return null;
	}

	public virtual string GetTitle()
	{
		return GetType().Name;
	}
}

public class Militarism : Policy { }
public class Fanaticism : Policy { }
public class Capitalist : Policy { }
public class Exploration : Policy { }

