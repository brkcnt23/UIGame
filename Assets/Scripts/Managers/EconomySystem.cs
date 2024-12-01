using UnityEngine;
public class EconomySystem
{
    private PlayerData playerData;

    public EconomySystem(PlayerData pd)
    {
        playerData = pd;
    }

    public void AddSilver(int gainedSilver)
    {
        playerData.Silver += gainedSilver;
        ConvertSilverToGold();
    }
    public void ConvertSilverToGold()
    {
        if (playerData.Silver >= 100)
        {
            int goldToAdd = playerData.Silver / 100;
            playerData.Gold += goldToAdd;
            playerData.Silver = playerData.Silver % 100;

            Debug.Log($"{goldToAdd} altın elde ettiniz. Kalan Gümüş: {playerData.Silver}");
        }
    }

}
