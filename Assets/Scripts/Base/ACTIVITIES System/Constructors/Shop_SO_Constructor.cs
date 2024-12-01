using UnityEngine;

public class Shop_SO_Constructor : SO_Base
{
    public Shop_SO_Constructor()
    {
        Type = SOTypes.CRAFT;

        ID = 0;
        Name = "New Shop";
        Description = "This is a new shop.";

        DC = 10;

        CompletionDay = 0;
        CompletionHour = 1;
        CompletionMinute = 0;

        Silver = 100;

        TargetStat = "Constitution";
        StatRewardMin = 1;
        StatRewardMax = 3;
    }
}