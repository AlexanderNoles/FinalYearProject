using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuestSlotControl : MonoBehaviour
{
	public GameObject mainUI;
	public TextMeshProUGUI titleLabel;
	public Button acceptQuestButton;

	public void Hide()
	{
		mainUI.SetActive(false);
	}

	public void Draw(string title, UnityAction onButtonClick)
	{
		mainUI.SetActive(true);
		titleLabel.text = title;

		acceptQuestButton.onClick.RemoveAllListeners();
		acceptQuestButton.onClick.AddListener(onButtonClick);
	}
}
