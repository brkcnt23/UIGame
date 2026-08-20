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
        if (questPanel == null)
        {
            return;
        }

        foreach (Transform child in questPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// The tavern panel can be enabled before a game is loaded (scene start), so
    /// everything it needs is checked before any quest is built.
    /// </summary>
    bool IsReadyToGenerate()
    {
        if (questPanel == null || questPrefab == null || QuestInfoPanel == null ||
            AcceptButton == null || DeclineButton == null || CancelButton == null || CompleteButton == null)
        {
            Debug.LogWarning("TavernQuestHandler: UI references are not assigned.");
            return false;
        }

        if (SettlementHandler.Instance == null || SettlementHandler.Instance.settlement == null ||
            SettlementHandler.Instance.settlement.Tavern == null ||
            SettlementHandler.Instance.settlement.Tavern.Quests == null)
        {
            // No settlement entered yet, nothing to show.
            return false;
        }

        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
        {
            return false;
        }

        return true;
    }

    void GenerateQuests()
    {
        if (!IsReadyToGenerate())
        {
            return;
        }

        Settlement currentSettlement = SettlementHandler.Instance.settlement;

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
        foreach (Quest_SO_Constructor quest in currentSettlement.Tavern.Quests)
        {
            if (quest == null)
                continue;

            questsToDisplay.Add(quest);
        }

        if (PlayerStatHandler.Instance.pd.Quests == null)
        {
            PlayerStatHandler.Instance.pd.Quests = new List<Quest_SO_Constructor>();
        }

        // Add player's active quests related to this settlement
        foreach (Quest_SO_Constructor playerQuest in PlayerStatHandler.Instance.pd.Quests)
        {
            if (playerQuest == null)
                continue;

            if (playerQuest.settlementID == currentSettlement.ID)
            {
                if (questsToDisplay.Contains(playerQuest))
                {
                    //change to quest in the list and tavern
                    questsToDisplay.Remove(playerQuest);
                    questsToDisplay.Add(playerQuest);
                    currentSettlement.Tavern.Quests.Remove(playerQuest);
                    currentSettlement.Tavern.Quests.Add(playerQuest);
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

                if (questButton.questName != null)
                    questButton.questName.text = quest.Name;

                if (questButton.HoursToComplete != null)
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
        if (_quest == null)
        {
            return;
        }

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

            if (selectedQuest.questType == QuestType.Location && MapHandler.Instance != null)
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
            if (selectedQuest.questType == QuestType.Location && MapHandler.Instance != null)
            {
                List<GameObject> childrenToRemove = new List<GameObject>();

                foreach (GameObject child in MapHandler.Instance.children)
                {
                    if (child == null)
                        continue;

                    SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

                    if (settlementButtonPointer == null || settlementButtonPointer.settlement == null)
                        continue;

                    if (settlementButtonPointer.settlement.Name == selectedQuest.questLocation)
                    {
                        childrenToRemove.Add(child);
                    }
                }

                foreach (GameObject child in childrenToRemove)
                {
                    SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

                    if (settlementButtonPointer != null)
                        MapHandler.Instance.RemoveQuestSettlement(settlementButtonPointer);
                }
            }
        }
    }
}