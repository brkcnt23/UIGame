using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

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

    private Quest_SO_Constructor selectedQuest;

    void OnEnable()
    {
        ClearQuests();
        GenerateQuests();
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

        // Assign event listeners once
        AcceptButton.onClick.AddListener(AcceptQuest);
        DeclineButton.onClick.AddListener(DeclineQuest);
        CancelButton.onClick.AddListener(CancelQuest);

        foreach (Quest_SO_Constructor quest in SettlementHandler.Instance.settlement.Tavern.Quests)
        {
            if (PlayerStatHandler.Instance.pd.Quests.Contains(quest))
            {
                continue;
            }

            Button questGO = Instantiate(questPrefab, questPanel.transform);
            QuestButton questButton = questGO.GetComponent<QuestButton>();
            Image questImage = questGO.GetComponent<Image>();

            if (questButton != null)
            {
                questButton.quest = quest;
                questButton.questName.text = quest.Name;
                questButton.HoursToComplete.text = $"{quest.hoursToComplete} hours";
            }

            if (quest.isTaken)
            {
                questGO.interactable = false;
                if (questImage != null)
                {
                    questImage.color = Color.gray;
                }

                AcceptButton.gameObject.SetActive(false);
                DeclineButton.gameObject.SetActive(false);
                CancelButton.gameObject.SetActive(true);
                questReward.gameObject.SetActive(false);
            }
            else
            {
                questGO.interactable = true;
                if (questImage != null)
                {
                    questImage.color = Color.white;
                }

                AcceptButton.gameObject.SetActive(true);
                DeclineButton.gameObject.SetActive(true);
                CancelButton.gameObject.SetActive(false);
                questReward.gameObject.SetActive(true);
            }

            if (quest.isCompleted)
            {
                questGO.interactable = true;
                if (questImage != null)
                {
                    questImage.color = Color.green;
                }

                Quest_SO_Constructor currentQuest = quest; // Capture current quest

                questGO.onClick.AddListener(() =>
                {
                    currentQuest.QuestComplete(PlayerStatHandler.Instance.pd);
                    SettlementHandler.Instance.settlement.Tavern.Quests.Remove(currentQuest);
                    ClearQuests();
                    GenerateQuests();
                });
            }
            else
            {
                Quest_SO_Constructor currentQuest = quest; // Capture current quest

                questGO.onClick.AddListener(() =>
                {
                    QuestInfoPanel.SetActive(true);
                    questName.text = currentQuest.Name;
                    questDescription.text = currentQuest.Description;
                    questReward.text = $"({currentQuest.Silver} silver)";
                    selectedQuest = currentQuest;
                });
            }
        }
    }

    void AcceptQuest()
    {
        // Implement accept quest logic
        if (selectedQuest != null)
        {
            selectedQuest.TryToTake(); // Call TryToTake on selected quest
            QuestInfoPanel.SetActive(false);
            ClearQuests();
            GenerateQuests();

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
    }

    void CancelQuest()
    {
        // Implement cancel quest logic
        QuestInfoPanel.SetActive(false);
        ClearQuests();
        GenerateQuests();

        if (selectedQuest != null)
        {
            selectedQuest.QuestCancel(PlayerStatHandler.Instance.pd);

            if (selectedQuest.questType == QuestType.Location)
            {

                // Remove quest from map
                foreach (GameObject child in MapHandler.Instance.children)
                {
                    SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

                    if (settlementButtonPointer.settlement.Name == selectedQuest.questLocation)
                    {
                        MapHandler.Instance.RemoveQuestSettlement(settlementButtonPointer);
                    }
                }
            }
        }
    }
}