using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_BaseSO : MonoBehaviour
{
    public List<SO_Base> JobSOs = new List<SO_Base>();

    private Job_SO_Constructor jobSO;
    private Quest_SO_Constructor questSO;
    private Event_SO_Constructor eventSO;
    void OnEnable()
    {
        foreach (SO_Base so in JobSOs)
        {
            if (so.Type == SOTypes.JOB)
            {
                jobSO = (Job_SO_Constructor)so;
            }
            else if (so.Type == SOTypes.QUEST)
            {
                questSO = (Quest_SO_Constructor)so;
            }
            else if (so.Type == SOTypes.EVENT)
            {
                eventSO = (Event_SO_Constructor)so;
            }
        }

        TestJobSO();
        TestQuestSO();
        TestEventSO();
    }

    void TestJobSO()
    {
        Print($"Job SO:\n Name: {jobSO.Name}\n Description: {jobSO.Description}\n DC: {jobSO.DC}\n Completion Time: {jobSO.CompletionHour}\n Reward: {jobSO.Reward}");
    }

    void TestQuestSO()
    {
        Print($"Quest SO:\n Name: {questSO.Name}\n Description: {questSO.Description}\n DC: {questSO.DC}\n Completion Time: {questSO.CompletionHour}\n Reward: {questSO.Reward}");
    }

    void TestEventSO()
    {
        Print($"Event SO:\n Name: {eventSO.Name}\n Description: {eventSO.Description}\n DC: {eventSO.DC}\n Completion Time: {eventSO.CompletionHour}\n Reward: {eventSO.Reward}");
    }

    void Print(string message)
    {
        Debug.Log($"{message}\nSender:\"{this.GetType().Name}\" class in \"{this.gameObject.name}\" object");
    }
}
