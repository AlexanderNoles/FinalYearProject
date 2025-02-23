using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class QuestUIControl : MonoBehaviour
{
	public static QuestUIControl instance;
	private QuestGiver targetData;
	private Transform targetDataTransform;

	public GameObject mainUI;
	public GameObject cantAcceptBlocker;
	public DraggableWindow mainWindowControl;

	[Header("Quest Slot Generation")]
	public RectTransform slotArea;
	public GameObject baseQuestSlotObject;
	public int slotCount = 5;
	public List<QuestSlotControl> questSlots = new List<QuestSlotControl>();

	[ContextMenu("Generate Quest Slots")]
	public void GenerateQuestSlots()
	{
		//Destroy all stored quests slots
		for (int i = 0; i < questSlots.Count;)
		{
			DestroyImmediate(questSlots[0].gameObject);
			questSlots.RemoveAt(0);
		}

		int startY = -60;
		int offsetPer = -125;

		for (int i = 0; i < slotCount; i++)
		{
			Vector3 anchoredPos = new Vector3(0, startY + (offsetPer * i), 0);

			QuestSlotControl newSlot = (PrefabUtility.InstantiatePrefab(baseQuestSlotObject, slotArea) as GameObject).GetComponent<QuestSlotControl>();
			(newSlot.transform as RectTransform).anchoredPosition3D = anchoredPos;

			questSlots.Add(newSlot);
		}
	}

	private void Awake()
	{
		instance = this;
	}

	public static void ToggleQuestUI(QuestGiver target, Transform targetsTransform)
	{
		if (target == null)
		{
			throw new System.Exception("Quest Giver is null!");
		}

		if (target.Equals(instance.targetData))
		{
			instance.CloseQuestUI();
		}
		else
		{       
			//Set target data
			instance.targetData = target;
			instance.targetDataTransform = targetsTransform;

			//Do draw
			instance.Draw();
		}
	}

	public static bool QuestUIOpen()
	{
		return instance != null && instance.mainUI.activeSelf;
	}

	private void Draw()
	{
		//Set main window active
		mainUI.SetActive(true);

		//Do window inital offset
		mainWindowControl.InitialOffset();

		//Run on quest UI opened
		targetData.OnQuestUIOpened();

		//Display datas quests
		DrawSlots();
	}

	//Seperated function from draw as when a quest is accepted we need to redraw all slots but not replace the window, regen held quests, etc.
	private void DrawSlots()
	{
		for (int i = 0; i < questSlots.Count; i++)
		{
			DrawSlot(i < targetData.heldQuests.Count, i);
		}
	}

	private void DrawSlot(bool active, int index)
	{
		if (!active)
		{
			questSlots[index].Hide();
		}
		else
		{
			questSlots[index].Draw(targetData.heldQuests[index].GetTitle(), () => { AcceptQuest(index); });
		}
	}

	private void AcceptQuest(int questIndex)
	{
		Quest quest = targetData.heldQuests[questIndex];

		//Get player quest data
		if (PlayerManagement.PlayerEntityExists())
		{
			PlayerQuests questData = PlayerManagement.GetQuests();

			if (questData.AttemptToAcceptQuest(quest))
			{
				targetData.heldQuests.RemoveAt(questIndex);
				//Redraw slots
				DrawSlots();
			}
		}
	}


	private void Update()
	{
		if (targetData != null)
		{
			if (UIHelper.CloseInteractionBasedUI(targetDataTransform) || targetData.heldQuests.Count == 0)
			{
				CloseQuestUI();
			}
			else
			{
				cantAcceptBlocker.SetActive(targetData.parent.Get().GetPlayerReputation() <= BalanceManagement.properInteractionAllowedThreshold);
			}
		}
	}

	public void CloseQuestUI()
	{
		mainUI.SetActive(false);
		//Null target data so toggle functions correctly
		targetData = null;
	}
}
