using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase
{
	//Item data storage
	public class ItemData
	{
		//Representation of a item loaded from the items file
		//Acts as a helper for getting data about items
		public Sprite icon;
		public string name;
		public string description;
		public string extraDescription;

		//Typically keys in the file are matched to actual variables but there should be a backup in the form of a dict in case
		//there is not a corresponding variable.

		public Dictionary<string, string> nonPredefinedKeyToValue = new Dictionary<string, string>();
	}

	public static Dictionary<int, ItemData> itemIDToItemData = new Dictionary<int, ItemData>();
	private static int currentItemIndex = 0;

	public static ItemData GetRandomItem()
	{
		//No items
		if (currentItemIndex == 0)
		{
			return null;
		}

		return itemIDToItemData[GetRandomItemIndex()];
	}

	public static int GetRandomItemIndex()
	{
		return Random.Range(0, currentItemIndex);
	}

	//

	[RuntimeInitializeOnLoadMethod]
	public static void LoadItemsFromFile()
	{
		currentItemIndex = 0;
		itemIDToItemData.Clear();
		Dictionary<string, string> aliasDict = new Dictionary<string, string>();

		//Load file from resources
		string mainFile = Resources.Load("items").ToString();

		//Sections of the file are seperated by curly brackets
		//When we encounter a opening bracket we assume we are in a section until we encounter a closing bracket

		//First split the file text into lines
		string[] lines = mainFile.Split('\n');

		bool inSection = false;
		string sectionKey = "";
		ItemData newItem = null;

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

#if !UNITY_EDITOR
				if (sectionKey.Contains("(debug)"))
				{
					inSection = false;
				}
#endif
			}
			else if (line.Contains("}"))
			{
				inSection = false;

				//Apply effects of section, currently only does item section here so aliases can reference other aliases
				if (newItem != null)
				{
					itemIDToItemData.Add(currentItemIndex, newItem);
					currentItemIndex++;

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
							newItem = new ItemData();
						}

						string key = PrepKeyString(parts[0]);
						string textBody = PrepTextBody(parts[1], aliasDict);

						//If key matches any item attribute key then we apply it to the current item being put into the database
						//if it doesn't match we put it into a general backup dictionary, all stat modifications go in here
						if (key.Contains("icon"))
						{
							//Load sprite from resources
							Sprite newIcon = Resources.Load<Sprite>($"icons/{textBody}");

							if (newIcon != null)
							{
								newItem.icon = newIcon;
							}
							else
							{
								Debug.LogWarning($"Icon path is invalid: {textBody}");
							}
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
						else
						{
							newItem.nonPredefinedKeyToValue.Add(key, textBody);
						}
					}
				}
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
