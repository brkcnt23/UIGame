using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JobUI : MonoBehaviour
{
    [Header("Stable Job Buttons")]
    [SerializeField] private Button helpMerchantsButton;
    [SerializeField] private Button helpScoutsButton;
    [SerializeField] private Button gatherHerbsButton;
    [SerializeField] private Button cuttingWoodsButton;
    [SerializeField] private Button laboringMinesButton;

    [Header("Settlement Specialization Jobs")]
    [SerializeField] private GameObject jobPanel;
    [SerializeField] private Transform jobContainer;
    [SerializeField] private GameObject jobPrefab;

    private List<Job_SO_Constructor> specializationJobs = new List<Job_SO_Constructor>();

    private void Start()
    {
        if (helpMerchantsButton != null)
        {
            helpMerchantsButton.onClick.RemoveAllListeners();
            helpMerchantsButton.onClick.AddListener(() => JobSystem.Instance.StartHelpMerchants());
        }

        if (helpScoutsButton != null)
        {
            helpScoutsButton.onClick.RemoveAllListeners();
            helpScoutsButton.onClick.AddListener(() => JobSystem.Instance.StartHelpScouts());
        }

        if (gatherHerbsButton != null)
        {
            gatherHerbsButton.onClick.RemoveAllListeners();
            gatherHerbsButton.onClick.AddListener(() => JobSystem.Instance.StartGatherHerbs());
        }

        if (cuttingWoodsButton != null)
        {
            cuttingWoodsButton.onClick.RemoveAllListeners();
            cuttingWoodsButton.onClick.AddListener(() => JobSystem.Instance.StartCuttingWoods());
        }

        if (laboringMinesButton != null)
        {
            laboringMinesButton.onClick.RemoveAllListeners();
            laboringMinesButton.onClick.AddListener(() => JobSystem.Instance.StartLaboringMines());
        }

        if (jobPanel != null)
            jobPanel.SetActive(false);

        if (JobSystem.Instance != null)
        {
            specializationJobs = JobSystem.Instance.GetAvailableJobs();
        }
        else
        {
            Debug.LogWarning("JobUI: JobSystem.Instance is null.");
        }
    }

    public void ToggleSpecializationJobUI()
    {
        if (jobPanel == null)
        {
            Debug.LogWarning("JobUI: jobPanel is null.");
            return;
        }

        jobPanel.SetActive(!jobPanel.activeSelf);

        if (jobPanel.activeSelf)
        {
            UpdateSpecializationJobUI();
        }
    }

    private void UpdateSpecializationJobUI()
    {
        if (jobContainer == null || jobPrefab == null)
        {
            Debug.LogWarning("JobUI: jobContainer or jobPrefab is null.");
            return;
        }

        foreach (Transform child in jobContainer)
        {
            Destroy(child.gameObject);
        }

        if (specializationJobs == null || specializationJobs.Count == 0)
        {
            Debug.Log("JobUI: No specialization jobs available.");
            return;
        }

        foreach (Job_SO_Constructor job in specializationJobs)
        {
            if (job == null) continue;

            GameObject newJob = Instantiate(jobPrefab, jobContainer);

            TMP_Text titleText = newJob.transform.Find("Title")?.GetComponent<TMP_Text>();
            TMP_Text descText = newJob.transform.Find("Description")?.GetComponent<TMP_Text>();
            Button startButton = newJob.transform.Find("StartButton")?.GetComponent<Button>();

            if (titleText != null)
                titleText.text = job.Name;

            if (descText != null)
                descText.text = $"{job.Description}\n{GetRewardInfo(job)}";

            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(() =>
                {
                    if (JobSystem.Instance != null)
                    {
                        JobSystem.Instance.StartJob(job);
                    }

                    ToggleSpecializationJobUI();
                });
            }
        }
    }

    private string GetRewardInfo(Job_SO_Constructor job)
    {
        if (job == null) return "Reward: Unknown";

        switch (job.Name)
        {
            case "Help the Merchants":
                return "Reward: Silver & Charisma XP";

            case "Help the Scouts":
                return "Reward: Silver & Dexterity XP";

            case "Cutting Woods":
                return "Reward: Wood & Strength XP";

            case "Laboring Mines":
                return "Reward: Stone, chance of Iron Ingot / Gold Nugget, Constitution XP";

            default:
                return $"Reward: {job.Silver} Silver, {job.StatRewardMin}-{job.StatRewardMax} {job.TargetStat} XP";
        }
    }
}