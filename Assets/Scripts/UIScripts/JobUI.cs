using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JobUI : MonoBehaviour
{
    [Header("Stable Job Buttons")]
    [SerializeField] private Button helpMerchantsButton;
    [SerializeField] private Button helpScoutsButton;
    [SerializeField] private Button cuttingWoodsButton;
    [SerializeField] private Button laboringMinesButton;

    [Header("Settlement Specialization Jobs")]
    [SerializeField] private GameObject jobPanel; // Panel for specialization jobs
    [SerializeField] private Transform jobContainer; // Parent object for job entries
    [SerializeField] private GameObject jobPrefab; // Prefab for specialization job entry

    private List<Job_SO_Constructor> specializationJobs;


    private void Start()
    {
        // Assign button listeners for stable jobs
        helpMerchantsButton.onClick.AddListener(() => JobSystem.Instance.StartHelpMerchants());
        helpScoutsButton.onClick.AddListener(() => JobSystem.Instance.StartHelpScouts());
        cuttingWoodsButton.onClick.AddListener(() => JobSystem.Instance.StartCuttingWoods());
        laboringMinesButton.onClick.AddListener(() => JobSystem.Instance.StartLaboringMines());

        // Hide specialization job panel by default
        jobPanel.SetActive(false);

        // Get available specialization jobs
        specializationJobs = JobSystem.Instance.GetAvailableJobs();
    }




    public void ToggleSpecializationJobUI()
    {
        jobPanel.SetActive(!jobPanel.activeSelf);
        if (jobPanel.activeSelf)
        {
            UpdateSpecializationJobUI();
        }
    }



    private void UpdateSpecializationJobUI()
    {
        // Clear previous entries
        foreach (Transform child in jobContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate specialization job entries using the prefab
        foreach (Job_SO_Constructor job in specializationJobs)
        {
            GameObject newJob = Instantiate(jobPrefab, jobContainer);

            // Set the title and description
            TMP_Text titleText = newJob.transform.Find("Title").GetComponent<TMP_Text>();
            TMP_Text descText = newJob.transform.Find("Description").GetComponent<TMP_Text>();
            Button startButton = newJob.transform.Find("StartButton").GetComponent<Button>();

            titleText.text = job.Name;
            descText.text = job.Description;

            startButton.onClick.AddListener(() =>
            {
                JobSystem.Instance.StartJob(job);
                ToggleSpecializationJobUI();
            });
        }
    }

    private void UpdateJobUI()
    {
        // Clear previous entries
        foreach (Transform child in jobContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate job entries
        foreach (Job_SO_Constructor job in specializationJobs)
        {
            GameObject newJob = Instantiate(jobPrefab, jobContainer);
            TMP_Text jobText = newJob.GetComponentInChildren<TMP_Text>();
            string rewardInfo = GetRewardInfo(job);

            jobText.text = $"{job.Name}\n{rewardInfo}";

            Button jobButton = newJob.GetComponentInChildren<Button>();
            jobButton.onClick.AddListener(() =>
            {
                JobSystem.Instance.StartJob(job);
                ToggleSpecializationJobUI();
            });
        }
    }

    private string GetRewardInfo(Job_SO_Constructor job)
    {
        switch (job.Name)
        {
            case "Help the Merchants":
                return "Reward: Silver & Charisma XP";
            case "Help the Scouts":
                return "Reward: Silver & Dexterity XP";
            case "Cutting Woods":
                return "Reward: Wood & Strength XP";
            case "Laboring Mines":
                return "Reward: Stone, Chance of Iron Ingot & Gold Nugget, Constitution XP";
            default:
                return $"Reward: {job.StatRewardMin}-{job.StatRewardMax} {job.TargetStat}";
        }
    }
}