using UnityEngine;

public class EconomySystem
{
    private readonly PlayerData playerData;

    public EconomySystem(PlayerData pd)
    {
        playerData = pd;
    }

    public Currency GetBalance()
    {
        return playerData.GetMoney();
    }

    public bool HasEnough(int gold, int silver)
    {
        return playerData.HasEnoughMoney(gold, silver);
    }

    public bool HasEnough(Currency amount)
    {
        return playerData.HasEnoughMoney(amount);
    }

    public void AddSilver(int gainedSilver)
    {
        playerData.AddMoney(0, gainedSilver);
        Debug.Log($"Added {gainedSilver} silver. New Balance: {playerData.GetMoney()}");
    }

    public void AddGold(int gainedGold)
    {
        playerData.AddMoney(gainedGold, 0);
        Debug.Log($"Added {gainedGold} gold. New Balance: {playerData.GetMoney()}");
    }

    public void AddMoney(int gold, int silver)
    {
        playerData.AddMoney(gold, silver);
        Debug.Log($"Added {gold} gold and {silver} silver. New Balance: {playerData.GetMoney()}");
    }

    public void AddMoney(Currency amount)
    {
        playerData.AddMoney(amount);
        Debug.Log($"Added {amount}. New Balance: {playerData.GetMoney()}");
    }

    public bool TrySpendMoney(int gold, int silver)
    {
        bool success = playerData.TrySpendMoney(gold, silver);

        if (!success)
        {
            Debug.LogWarning($"Not enough money. Tried to spend {gold} gold and {silver} silver. Current Balance: {playerData.GetMoney()}");
            return false;
        }

        Debug.Log($"Spent {gold} gold and {silver} silver. Remaining Balance: {playerData.GetMoney()}");
        return true;
    }

    public bool TrySpendMoney(Currency amount)
    {
        return TrySpendMoney(amount.Gold, amount.Silver);
    }
}