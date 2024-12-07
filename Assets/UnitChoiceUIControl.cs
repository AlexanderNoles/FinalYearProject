using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using TMPro;
using UnityEngine.UI;

public class UnitChoiceUIControl : MonoBehaviour
{
	public GameObject baseChoiceObject;

	public void Setup(ShipModulesControl parent)
	{
		//Init
		//Get all scripts that inheirt from unit base
		List<Type> types = Assembly.GetAssembly(typeof(PlayerShipUnitBase)).GetTypes().Where(
			TheType => 
			TheType.IsClass && 
			!TheType.IsAbstract && 
			TheType.IsSubclassOf(typeof(PlayerShipUnitBase)) &&
			TheType != typeof(EmptyUnit)
			).ToList();

		int y = -60;
		foreach (Type type in types)
		{
			RectTransform rect = (RectTransform)Instantiate(baseChoiceObject, transform).transform;
			rect.gameObject.SetActive(true);

			rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
			y -= 110;

			TextMeshProUGUI label = rect.GetChild(1).GetComponent<TextMeshProUGUI>();
			label.text = System.Text.RegularExpressions.Regex.Replace(type.Name, "[A-Z]", " $0").Trim();
			
			Image image = rect.GetChild(2).GetComponent<Image>();
			image.sprite = VisualDatabase.GetUnitSpriteFromType(type);

			Button button = rect.GetChild(3).GetComponent<Button>();
			button.onClick.AddListener(delegate
			{
				parent.ReplaceSelectedUnitWithUnitOfType(type);
			});
		}
	}
}
