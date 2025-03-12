using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static ItemDatabase;

public static class ItemHelper
{
	public static Item GetItemByTotalIndex(int index)
	{
		//Iterate through rarity bands
		foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
		{
			if (itemRarityToCollections.TryGetValue(rarity, out ItemCollection collection))
			{
				//In this rarity band
				if (index < collection.items.Count) 
				{
					return collection.items[index];
				}
				else
				{
					index -= collection.items.Count;
				}
			}
		}

		return null;
	}

	public static Item GetRandomItemOfRarity(ItemRarity rarity)
	{
		if (itemRarityToCollections.ContainsKey(rarity))
		{
			return itemRarityToCollections[rarity].GetRandomItem();
		}

		Debug.Log("No items of rarity!");
		return null;
	}

	public static Item GetItemForGeneralPurpose()
	{
		float chance = UnityEngine.Random.Range(0.0f, 1.0f);

		if (chance > 0.9f)
		{
			return GetRandomItemOfRarity(ItemRarity.Rare);
		}
		else
		{
			return GetRandomItemOfRarity(ItemRarity.Basic);
		}
	}

	private static Dictionary<string, Type> stringToTargetTypes = new Dictionary<string, Type>();

	public static ItemBase Wrap(Item targetData)
	{
		if (targetData == null)
		{
			throw new ArgumentNullException("ItemData to wrap cannot be null");
		}

		//For most items this will simply attach it to a typical ItemBase
		//and then return
		//For certain items where custom/bespoke code is needed an instance of the required
		//subtype will be returned instead
		//For this to work the types need to be known. To maintain dynamism we need to find and compile these at runtime
		if (stringToTargetTypes.ContainsKey(targetData.targetSubtypeName))
		{
			return (Activator.CreateInstance(stringToTargetTypes[targetData.targetSubtypeName]) as ItemBase).Setup(targetData);
		}
		else
		{
			return new ItemBase().Setup(targetData);
		}
	}

	[RuntimeInitializeOnLoadMethod]
	public static void GenerateNameToItemSubTypeDict()
	{
		stringToTargetTypes.Clear();
		Type[] targetTypes = Assembly.GetAssembly(typeof(ItemBase)).GetTypes().Where(t => t.BaseType == typeof(ItemBase)).ToArray();

		foreach (Type type in targetTypes)
		{
			stringToTargetTypes.Add(type.Name, type);
		}
	}
}
