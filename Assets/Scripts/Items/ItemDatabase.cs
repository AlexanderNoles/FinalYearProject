using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class ItemDatabase
{
	private const bool debugItemsEnabled = false;

	public enum ItemClass
	{
		Divine,
		Arcane,
		Domination,
		Void
	}

	public enum ItemRarity
	{
		Basic,
		Rare,
		Exalted
	}

	//Item data storage
	public class Item : IDisplay
	{
		//Representation of a item loaded from the items file
		//Acts as a helper for getting data about items
		public Sprite icon;
		public string targetSubtypeName = "";
		public string name;
		public string description;
		public string extraDescription;
		public string itemTypeDeclaration;
		public float basePrice;
		public ItemClass itemClass;
		public ItemRarity itemRarity = ItemRarity.Basic;

		public string statContributorDescriptions;

		public void GenerateStatContributorDescriptions()
		{
			//Generate item additional extra description
			statContributorDescriptions = "\n\n";

			foreach (KeyValuePair<string, (StatContributor.Type, float)> entry in keyToStat)
			{
				string modifierSign = "x";

				if (entry.Value.Item1 == StatContributor.Type.Addition)
				{
					modifierSign = entry.Value.Item2 >= 0.0f ? "+" : "";
				}

				statContributorDescriptions = $"{statContributorDescriptions}\n{modifierSign}{entry.Value.Item2} {GetKeyAsTitle(entry.Key)}";
			}
		}

		public Dictionary<string, (StatContributor.Type, float)> keyToStat = new Dictionary<string, (StatContributor.Type, float)>();
		

		//DISPLAY METHODS
		public string GetTitle()
		{
			return name;
		}

		public string GetDescription()
		{
			return $"<color=#949494>[{itemClass}]</color>\n\n" + description;
		}

		public Sprite GetIcon()
		{
			return icon;
		}

		public string GetExtraInformation()
		{
			return extraDescription + statContributorDescriptions;
		}
	}

	public class ItemCollection
	{
		public List<Item> items = new List<Item>();

		public void Add(Item item)
		{
			items.Add(item);
		}

		public Item GetRandomItem()
		{
			if (items.Count == 0)
			{
				return null;
			}

			return items[GetRandomItemIndex()];
		}

		public int GetRandomItemIndex()
		{
			return Random.Range(0, items.Count);
		}
	}

	public static string GetKeyAsTitle(string inputKey)
	{
		if (inputKey.Length < 2)
		{
			throw new System.Exception("Input key is below expected length, is this the key? If so, is this key appropriately named?");
		}

		//Add strings before capital letters
		//Then replace first character with capital
		inputKey = Regex.Replace(inputKey, "[A-Z]", " $0");

		return $"{char.ToUpper(inputKey[0])}{inputKey[1..]}";
	}

	public static Dictionary<ItemRarity, ItemCollection> itemRarityToCollections = new Dictionary<ItemRarity, ItemCollection>();
	public static int totalLoadedItemCount;

	public static int GetTotalItemCount()
	{
		return totalLoadedItemCount;
	}
	//

	[RuntimeInitializeOnLoadMethod]
	public static void LoadItemsFromFile()
	{
		totalLoadedItemCount = 0;
		itemRarityToCollections.Clear();
		string currentItemTypeDeclaration = "";
        Dictionary<string, string> aliasDict = new Dictionary<string, string>();

		//Load file from resources
		string mainFile = Resources.Load("items").ToString();

		//Sections of the file are seperated by curly brackets
		//When we encounter a opening bracket we assume we are in a section until we encounter a closing bracket

		//First split the file text into lines
		string[] lines = mainFile.Split('\n');

		bool inSection = false;
		string sectionKey = "";
		Item newItem = null;

		foreach (string line in lines)
		{
			if (line.Contains("//"))
			{
				//Comment line
				continue;
			}
			//SECTION ENTRANCE AND EXIT CONTROL
			else if (line.Contains("{"))
			{
				inSection = true;

				//Remove section start token and whitespace
				sectionKey = PrepKeyString(line.Replace("{", ""));

				if (!Application.isEditor || !debugItemsEnabled)
				{
					if (sectionKey.Contains("(debug)"))
					{
						inSection = false;
					}
				}

				if (sectionKey.Contains("(disabled)"))
				{
					inSection = false;
				}
			}
			else if (line.Contains("}"))
			{
				inSection = false;

				//Apply effects of section at its end, 
				//currently only does item section here so aliases can reference other aliases defined in the same section
				if (newItem != null)
				{
					newItem.itemTypeDeclaration = currentItemTypeDeclaration;
					newItem.GenerateStatContributorDescriptions();

					if (!itemRarityToCollections.ContainsKey(newItem.itemRarity))
					{
						itemRarityToCollections[newItem.itemRarity] = new ItemCollection();
					}

					itemRarityToCollections[newItem.itemRarity].Add(newItem);
					totalLoadedItemCount++;
					newItem = null;
				}
				//
			}
			//Section control
			else if (inSection)
			{
				if (sectionKey.Contains("aliases"))
				{
					//This is an aliases definition section
					//split input string based on ":" character
					string[] parts = GetKeyAndBody(line);

					if (parts.Length > 0)
					{
						string key = PrepKeyString(parts[0]);

						aliasDict.Add(key, PrepTextBody(parts[1], aliasDict));
					}
				}
				else if (sectionKey.Contains("item"))
				{
					//This is an item definition section

					string[] parts = GetKeyAndBody(line);

					if (parts.Length > 0)
					{
						if (newItem == null)
						{
							newItem = new Item();
						}

						string key = PrepKeyString(parts[0], false);
						string textBody = PrepTextBody(parts[1], aliasDict);

						//If key matches any item attribute key then we apply it to the current item being put into the database
						//if it doesn't match we put it into a general backup dictionary, all stat modifications go in here
						if (key.Contains("icon"))
						{
							//Load sprite from resources
							VisualDatabase.LoadIconFromResources(textBody, out newItem.icon);
						}
						else if (key.Contains("target"))
						{
							newItem.targetSubtypeName = textBody;
						}
						else if (key.Contains("rarity"))
						{
							newItem.itemRarity = (ItemRarity)int.Parse(textBody);
						}
						else if (key.Contains("name"))
						{
							newItem.name = textBody;
						}
						else if (key.Contains("description"))
						{
							newItem.description = textBody;
						}
						else if (key.Contains("extra"))
						{
							newItem.extraDescription = textBody;
						}
						else if (key.Contains("price"))
						{
							newItem.basePrice = float.Parse(textBody);
						}
						else if (key.Contains("class"))
						{
							newItem.itemClass = (ItemClass)int.Parse(textBody);
						}
						else
						{
							StatContributor.Type type = StatContributor.Type.Addition;

							if (textBody.Contains("xx"))
							{
								type = StatContributor.Type.FinalMultiplier;
							}
							else if (textBody.Contains("x"))
							{
								type = StatContributor.Type.BaseMultiplier;
							}

							textBody = textBody.Replace("x", "");

							newItem.keyToStat.Add(key, (type, float.Parse(textBody)));
						}
					}
				}
			}
			else if (line.Contains("###"))
			{
				//Set current item type declaration
				currentItemTypeDeclaration = PrepKeyString(line.Replace("#", ""), false);
            }
		}
	}

	private static string[] GetKeyAndBody(string input)
	{
		return input.Split(':', 2);
	}

	private static string PrepKeyString(string input, bool makeLower = true)
	{
		string output = input.Replace(@"\s", "").Replace("\t", "").Replace(" ", "");

		if (makeLower)
		{
			output = output.ToLower();
		}

		return output;
	}

	private static string PrepTextBody(string input, Dictionary<string, string> aliasDict)
	{
		char[] chars = input.ToCharArray();
		string returnString = "";
		//Need to move through the string
		//Once a "" is found we start adding to output
		//If we find a * we enter into alias mode and construct an alias string, when we encounter another * we see if the constructed alias string is a valid allias
		//if it is we add the corresponding text from aliasDict

		bool addingToString = false;
		bool constructingAliasKey = false;
		string currentAliasKey = "";

		for (int i = 0; i < chars.Length; i++)
		{
			if (chars[i] == '"')
			{
				if (addingToString)
				{
					//Currently just break and return
					break;
				}
				else
				{
					addingToString = true;
				}
			}
			else if (addingToString)
			{
				if (chars[i] == '*')
				{
					if (constructingAliasKey)
					{
						constructingAliasKey = false;

						//Add alias to output
						currentAliasKey = PrepKeyString(currentAliasKey);

						if (aliasDict.ContainsKey(currentAliasKey))
						{
							returnString += aliasDict[currentAliasKey];
						}
						else
						{
							returnString += $"{{ BROKEN ALLIAS: {currentAliasKey} }}";
						}
					}
					else
					{
						constructingAliasKey = true;
						currentAliasKey = "";
					}
				}
				else
				{
					if (constructingAliasKey)
					{
						currentAliasKey += chars[i];
					}
					else
					{
						returnString += chars[i];
					}
				}
			}
		}

		return returnString;
	}
}
