using System.Collections.Generic;

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

        TargetStat = StatType.Constitution;
        StatRewardMin = 1;
        StatRewardMax = 3;
    }
    public List<Item> Items = new List<Item>();
}