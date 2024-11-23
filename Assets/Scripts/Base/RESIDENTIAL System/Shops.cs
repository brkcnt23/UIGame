using UnityEngine;

public enum ShopTypes
{
    Blacksmith,
    Tanner,
    Carpenter,
    Alchemist,
    Mason,
    defaultShop
}
[System.Serializable]
public class Shops : Residentials
{
    public ShopTypes ShopType;
    public Shops()
    {
        ShopType = ShopTypes.defaultShop;
    }
}