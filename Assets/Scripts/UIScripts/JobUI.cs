using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JobUI : MonoBehaviour
{
    [SerializeField] private GameObject jobPanel; // Panel for job UI
    [SerializeField] private Transform jobContainer; // Parent object for job entries
    [SerializeField] private GameObject jobPrefab; // Prefab for job entry
    private List<Job_SO_Constructor> jobs;

    private void Start()
    {
        jobPanel.SetActive(false); // Hide job panel by default
        jobs = JobManager.Instance.GetAvailableJobs();
    }

    public void ToggleJobUI()
    {
        jobPanel.SetActive(!jobPanel.activeSelf);
        if (jobPanel.activeSelf)
        {
            UpdateJobUI();
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
        foreach (Job_SO_Constructor job in jobs)
        {
            GameObject newJob = Instantiate(jobPrefab, jobContainer);
            TMP_Text jobText = newJob.GetComponentInChildren<TMP_Text>();
            jobText.text = $"{job.Name}\nReward: {job.StatRewardMin}-{job.StatRewardMax} {job.TargetStat}";

            Button jobButton = newJob.GetComponentInChildren<Button>();
            jobButton.onClick.AddListener(() =>
            {
                JobManager.Instance.StartJob(job);
                ToggleJobUI();
            });
        }
    }
}
