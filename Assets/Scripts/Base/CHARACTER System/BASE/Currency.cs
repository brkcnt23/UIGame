using UnityEngine;

[System.Serializable]
public struct Currency
{
    public int Gold;    // Amount of gold
    public int Silver;  // Amount of silver

    // Constructor to initialize gold and silver
    public Currency(int gold, int silver)
    {
        Gold = gold;
        Silver = silver;
        Normalize();
    }
    public static Currency operator *(Currency currency, int multiplier)
    {
        return new Currency(currency.Gold * multiplier, currency.Silver * multiplier);
    }

    // Normalize ensures Silver is less than 100, converting overflow to Gold
    private void Normalize()
    {
        if (Silver >= 100)
        {
            Gold += Silver / 100;
            Silver %= 100;
        }
        else if (Silver < 0 && Gold > 0)
        {
            int borrowGold = (Mathf.Abs(Silver) + 99) / 100; // Borrow gold to cover negative silver
            Gold -= borrowGold;
            Silver += borrowGold * 100;
        }
    }

    // Add currency
    public void Add(int gold, int silver)
    {
        Gold += gold;
        Silver += silver;
        Normalize();
    }

    // Subtract currency (ensures no negative balance)
    public void Subtract(int gold, int silver)
    {
        Silver -= silver;
        Gold -= gold;

        if (Silver < 0)
        {
            int borrowGold = (Mathf.Abs(Silver) + 99) / 100;
            Gold -= borrowGold;
            Silver += borrowGold * 100;
        }

        if (Gold < 0)
        {
            Gold = 0;
            Silver = 0;
            Debug.LogError("Not enough currency to complete the transaction.");
        }
    }

    // Check if there is enough currency
    public bool HasEnough(int gold, int silver)
    {
        if (Gold > gold || (Gold == gold && Silver >= silver))
        {
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        return $"{Gold} Gold, {Silver} Silver";
    }
}
