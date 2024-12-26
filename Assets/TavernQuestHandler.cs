using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class TavernQuestHandler : MonoBehaviour
{
    public GameObject questPanel;
    public Button questPrefab;
    public GameObject QuestInfoPanel;
    public TMP_Text questName;
    public TMP_Text questDescription;
    public TMP_Text questReward;
    public Button AcceptButton;
    public Button DeclineButton;
    public Button CancelButton;
    public Button CompleteButton;

    private Quest_SO_Constructor selectedQuest;

    void OnEnable()
    {
        RefreshQuests();
    }

    void ClearQuests()
    {
        foreach (Transform child in questPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void GenerateQuests()
    {
        QuestInfoPanel.SetActive(false);

        // Clear existing listeners
        AcceptButton.onClick.RemoveAllListeners();
        DeclineButton.onClick.RemoveAllListeners();
        CancelButton.onClick.RemoveAllListeners();
        CompleteButton.onClick.RemoveAllListeners();

        // Assign event listeners once
        AcceptButton.onClick.AddListener(AcceptQuest);
        DeclineButton.onClick.AddListener(DeclineQuest);
        CancelButton.onClick.AddListener(CancelQuest);
        CompleteButton.onClick.AddListener(CompleteQuest);

        List<Quest_SO_Constructor> questsToDisplay = new List<Quest_SO_Constructor>();

        // Add quests from the settlement's Tavern
        foreach (Quest_SO_Constructor quest in SettlementHandler.Instance.settlement.Tavern.Quests)
        {
            questsToDisplay.Add(quest);
        }

        // Add player's active quests related to this settlement
        foreach (Quest_SO_Constructor playerQuest in PlayerStatHandler.Instance.pd.Quests)
        {
            if (playerQuest.settlementID == SettlementHandler.Instance.settlement.ID)
            {
                if (questsToDisplay.Contains(playerQuest))
                {
                    //change to quest in the list and tavern
                    questsToDisplay.Remove(playerQuest);
                    questsToDisplay.Add(playerQuest);
                    SettlementHandler.Instance.settlement.Tavern.Quests.Remove(playerQuest);
                    SettlementHandler.Instance.settlement.Tavern.Quests.Add(playerQuest);
                }
            }
        }

        foreach (Quest_SO_Constructor quest in questsToDisplay)
        {
            Button questGO = Instantiate(questPrefab, questPanel.transform);
            QuestButton questButton = questGO.GetComponent<QuestButton>();
            Image questImage = questGO.GetComponent<Image>();

            if (questButton != null)
            {
                questButton.quest = quest;
                questButton.questName.text = quest.Name;
                questButton.HoursToComplete.text = $"{quest.hoursToComplete} hours";
            }

            Quest_SO_Constructor currentQuest = quest; // Capture current quest

            if (PlayerStatHandler.Instance.pd.Quests.Contains(quest))
            {
                if (quest.isCompleted)
                {
                    questGO.interactable = true;
                    if (questImage != null)
                    {
                        questImage.color = Color.green;
                    }

                    questGO.onClick.AddListener(() =>
                    {
                        QuestInfoPanel.SetActive(true);
                        selectedQuest = currentQuest;
                        SetUpInfoPanel(selectedQuest);
                    });
                }
                else
                {
                    questGO.interactable = true;
                    if (questImage != null)
                    {
                        questImage.color = Color.yellow;
                    }

                    questGO.onClick.AddListener(() =>
                    {
                        QuestInfoPanel.SetActive(true);
                        selectedQuest = currentQuest;
                        SetUpInfoPanel(selectedQuest);
                    });
                }
            }
            else
            {
                if (quest.isTaken)
                {
                    questGO.interactable = false;
                    if (questImage != null)
                    {
                        questImage.color = Color.gray;
                    }
                }
                else
                {
                    questGO.interactable = true;
                    if (questImage != null)
                    {
                        questImage.color = Color.white;
                    }
                }

                questGO.onClick.AddListener(() =>
                {
                    QuestInfoPanel.SetActive(true);
                    questName.text = currentQuest.Name;
                    questDescription.text = currentQuest.Description;
                    selectedQuest = currentQuest;
                    SetUpInfoPanel(selectedQuest);
                });
            }
        }
    }

    public void SetUpInfoPanel(Quest_SO_Constructor _quest)
    {
        if (_quest.isTaken)
        {
            questName.text = _quest.Name + " (Active)";
            AcceptButton.gameObject.SetActive(false);
            DeclineButton.gameObject.SetActive(false);
            CancelButton.gameObject.SetActive(true);
            CompleteButton.gameObject.SetActive(false);

            if (_quest.isCompleted)
            {
                questReward.text = $"Reward: {_quest.Silver} silver, {_quest.Experience} experience";
                AcceptButton.gameObject.SetActive(false);
                DeclineButton.gameObject.SetActive(false);
                CancelButton.gameObject.SetActive(false);
                CompleteButton.gameObject.SetActive(true);

            }
        }
        else
        {
            questName.text = _quest.Name;
            AcceptButton.gameObject.SetActive(true);
            DeclineButton.gameObject.SetActive(true);
            CancelButton.gameObject.SetActive(false);
            CompleteButton.gameObject.SetActive(false);
        }

        questDescription.text = _quest.Description;
    }

    void RefreshQuests()
    {
        ClearQuests();
        GenerateQuests();
    }
    void CompleteQuest()
    {
        // Implement complete quest logic
        if (selectedQuest != null)
        {
            selectedQuest.QuestComplete(PlayerStatHandler.Instance.pd); // Call CompleteQuest on selected quest
            QuestInfoPanel.SetActive(false);
            RefreshQuests();
        }
    }

    void AcceptQuest()
    {
        // Implement accept quest logic
        if (selectedQuest != null)
        {
            selectedQuest.TryToTake(); // Call TryToTake on selected quest
            QuestInfoPanel.SetActive(false);
            RefreshQuests();

            if (selectedQuest.questType == QuestType.Location)
            {
                Settlement questsSettlement = new Settlement(selectedQuest);
                MapHandler.Instance.AddQuestSettlement(questsSettlement, ref selectedQuest.questLocationCoordinates);
                
            }
        }
    }

    void DeclineQuest()
    {
        QuestInfoPanel.SetActive(false);
        ClearQuests();
        GenerateQuests();
    }

    void CancelQuest()
    {
        if (selectedQuest != null)
        {
            selectedQuest.QuestCancel(PlayerStatHandler.Instance.pd);

            QuestInfoPanel.SetActive(false);
            RefreshQuests();
            if (selectedQuest.questType == QuestType.Location)
            {
                List<GameObject> childrenToRemove = new List<GameObject>();

                foreach (GameObject child in MapHandler.Instance.children)
                {
                    SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

                    if (settlementButtonPointer.settlement.Name == selectedQuest.questLocation)
                    {
                        childrenToRemove.Add(child);
                    }
                }

                foreach (GameObject child in childrenToRemove)
                {
                    SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();
                    MapHandler.Instance.RemoveQuestSettlement(settlementButtonPointer);
                }
            }
        }
    }
}