using UnityEngine;

/// <summary>
/// One place that decides what anything costs.
///
/// A single base price per item would make trade pointless: buying and selling
/// in the same town would be a pure loss, and buying in one town and selling in
/// the next would be exactly the same loss. Prices have to move with the place.
///
/// Four things move a price:
///
///   1. The shop's own margin      — buy low, sell high, always
///   2. Settlement wealth          — rich towns pay more and charge more
///   3. Local supply               — a mining village sells ore cheap
///   4. The player's Charisma      — haggling, and it finally gives CHA a job
///
/// The spread is deliberately wide enough that carrying salt from where it is
/// common to where it is not turns a profit, and narrow enough that it is not
/// a money printer.
/// </summary>
public static class PricingSystem
{
    // A shop never pays more than this fraction of what it charges. Without a
    // floor on the spread, a high-Charisma player could buy and sell in the
    // same shop forever.
    private const float MinimumSpread = 0.25f;

    /// <summary>Silver a shop of a given level is expected to keep in the till.</summary>
    private const float ExpectedTillPerLevel = 5000f;

    /// <summary>Units of one item past which a shop would rather have coin than more of it.</summary>
    private const int Overstocked = 12;

    /// <summary>What the player pays the shop for one unit.</summary>
    public static int GetBuyPrice(ItemSO item, Shops shop, Settlement settlement, PlayerData player)
    {
        if (item == null) return 0;

        float price = BaseSilver(item);

        price *= shop != null ? Mathf.Max(0.1f, shop.SellMultiplier) : 1.15f;
        price *= WealthFactor(settlement);
        price *= SupplyFactor(item, settlement);
        price *= ShopFactor(item, shop, buying: true);
        price *= HaggleFactor(player, buying: true);

        return Mathf.Max(1, Mathf.RoundToInt(price));
    }

    /// <summary>What the shop pays the player for one unit.</summary>
    public static int GetSellPrice(ItemSO item, Shops shop, Settlement settlement, PlayerData player)
    {
        if (item == null) return 0;

        float price = BaseSilver(item);

        price *= shop != null ? Mathf.Max(0.05f, shop.BuyMultiplier) : 0.5f;
        price *= WealthFactor(settlement);

        // Supply works the other way round when selling: a place drowning in
        // ore will not pay you well for more of it.
        price /= Mathf.Max(0.5f, SupplyFactor(item, settlement));
        price *= ShopFactor(item, shop, buying: false);
        price *= HaggleFactor(player, buying: false);

        int sell = Mathf.Max(1, Mathf.RoundToInt(price));
        int buy = GetBuyPrice(item, shop, settlement, player);

        // Enforce the spread so instant resale always loses money.
        return Mathf.Min(sell, Mathf.RoundToInt(buy * (1f - MinimumSpread)));
    }

    /// <summary>Price for a specific instance, including its rolled quality.</summary>
    public static int GetBuyPrice(Item instance, ItemSO template, Shops shop,
                                  Settlement settlement, PlayerData player)
    {
        int unit = GetBuyPrice(template, shop, settlement, player);
        return Mathf.Max(1, Mathf.RoundToInt(unit * QualityFactor(instance)));
    }

    public static int GetSellPrice(Item instance, ItemSO template, Shops shop,
                                   Settlement settlement, PlayerData player)
    {
        int unit = GetSellPrice(template, shop, settlement, player);
        return Mathf.Max(1, Mathf.RoundToInt(unit * QualityFactor(instance)));
    }

    // -----------------------------------------------------------------

    private static float BaseSilver(ItemSO item)
        => item.goldValue * 100f + item.silverValue;

    private static float QualityFactor(Item instance)
    {
        if (instance == null) return 1f;

        int q = Mathf.Clamp(instance.Quality, 0, 4);
        return ItemRules.QualityMultipliers[q] / 100f;
    }

    /// <summary>
    /// What separates two shops standing in the same street.
    ///
    /// The settlement sets the market; a shop moves a few percent off it, and
    /// always for a reason the player can read off the counter. Three things
    /// move it, and none of them is a dice roll — a random markup teaches the
    /// player nothing and makes them check every stall before every purchase.
    ///
    ///   1. The premises   — a bigger shop pays more rent and charges for it
    ///   2. The till       — a shopkeeper short of coin discounts to raise some
    ///   3. The shelf      — a stall drowning in iron sells iron cheap
    ///
    /// The spread is deliberately narrow. The point is that the same iron costs
    /// 49 here and 52 next door, not that one shop is a bargain and the other a
    /// robbery: a wide spread would turn every purchase into a tour of the town.
    /// </summary>
    private static float ShopFactor(ItemSO item, Shops shop, bool buying)
    {
        if (shop == null) return 1f;

        return OverheadFactor(shop, buying)
             * TillFactor(shop, buying)
             * StockFactor(item, shop, buying);
    }

    /// <summary>
    /// Rent. A level 5 premises on the main street costs more to keep than a
    /// level 1 stall, and that lands on the price tag.
    ///
    /// Only when the player is buying. The shopkeeper's overheads are not a
    /// reason for them to pay you more, and a levelled shop already rewards the
    /// player through the stock it is allowed to carry.
    /// </summary>
    private static float OverheadFactor(Shops shop, bool buying)
    {
        if (!buying) return 1f;

        int level = Mathf.Clamp(shop.level, 1, 10);
        return 1f + (level - 1) * 0.02f;
    }

    /// <summary>
    /// The coin in the till, against what a shop that size ought to be holding.
    ///
    /// A shopkeeper who has run their money into stock needs it back, so they
    /// shade prices down to shift goods. One sitting on a full chest can wait
    /// for their price, and can pay properly for what the player brings in.
    /// </summary>
    private static float TillFactor(Shops shop, bool buying)
    {
        float expected = Mathf.Max(1, shop.level) * ExpectedTillPerLevel;
        float held = shop.Cash.Gold * 100f + shop.Cash.Silver;
        float ratio = held / expected;

        if (ratio < 0.5f) return buying ? 0.96f : 0.94f;   // needs coin: sells cheap, pays badly
        if (ratio > 2.0f) return buying ? 1.02f : 1.03f;   // flush: holds firm, pays well

        return 1f;
    }

    /// <summary>
    /// How much of this particular thing is already on the shelf.
    ///
    /// This is the volume trade the player can actually exploit: a shop with
    /// twelve bars of iron would rather have coin than a thirteenth bar, and
    /// will not pay much for one either.
    /// </summary>
    private static float StockFactor(ItemSO item, Shops shop, bool buying)
    {
        if (item == null || shop.ItemEntries == null) return 1f;

        int quantity = 0;

        foreach (var entry in shop.ItemEntries)
            if (entry != null && entry.ItemId == item.ID)
                quantity += entry.Quantity;

        if (quantity >= Overstocked) return buying ? 0.96f : 0.92f;
        if (quantity <= 1)           return buying ? 1.05f : 1f;

        return 1f;
    }

    /// <summary>
    /// Rich settlements charge more and pay more. Range is kept modest —
    /// roughly 0.85 to 1.20 — so wealth nudges the market rather than
    /// making one city the only place worth trading.
    /// </summary>
    private static float WealthFactor(Settlement settlement)
    {
        if (settlement == null) return 1f;

        float gold = settlement.Wealth.Gold;
        if (gold <= 0f) return 0.85f;

        // Settlement wealth spans two orders of magnitude — Mege sits at
        // 1,200 gold and Evoynir at 120,000. A linear reading would saturate
        // at both ends and every town in between would price identically, so
        // the curve is logarithmic: each tenfold rise in wealth moves the
        // price by the same amount.
        //
        //   1,200g  -> 0.87    a hamlet, little coin about
        //   9,000g  -> 1.00    an ordinary village
        //   60,000g -> 1.13    a prosperous town
        //  120,000g -> 1.20    the city
        float t = Mathf.InverseLerp(Mathf.Log10(1000f), Mathf.Log10(150000f), Mathf.Log10(gold));
        return Mathf.Lerp(0.85f, 1.20f, t);
    }

    /// <summary>
    /// Local abundance. A settlement tagged for mining sells ore and ingots
    /// cheaply; one tagged for forestry does the same for timber.
    ///
    /// Below 1 means cheap here. This is the mechanism that makes a trade
    /// route worth walking.
    /// </summary>
    private static float SupplyFactor(ItemSO item, Settlement settlement)
    {
        if (item == null || settlement?.SettlementTags == null) return 1f;

        var tags = settlement.SettlementTags;

        switch (item.category)
        {
            case ItemCategory.Resource:
            case ItemCategory.CraftingMaterial:
                if (tags.Contains("mining") && IsMetal(item))     return 0.70f;
                if (tags.Contains("forestry") && IsWood(item))    return 0.70f;
                if (tags.Contains("pastoral") && IsAnimal(item))  return 0.70f;
                if (tags.Contains("quarry") && IsStone(item))     return 0.70f;
                if (tags.Contains("remote"))                      return 1.35f;
                return 1f;

            case ItemCategory.Consumable:
            case ItemCategory.Potion:
                if (tags.Contains("farming")) return 0.80f;
                if (tags.Contains("famine"))  return 2.00f;
                if (tags.Contains("remote"))  return 1.30f;
                return 1f;

            case ItemCategory.TradeGood:
                // Trade goods are the point: expensive where rare, cheap at
                // the source.
                if (tags.Contains("trade_hub")) return 0.80f;
                if (tags.Contains("remote"))    return 1.50f;
                return 1f;

            default:
                return 1f;
        }
    }

    /// <summary>
    /// Charisma haggling. Modifier of +4 (CHA 18) is roughly 8% either way,
    /// plus whatever traits and companions contribute.
    /// </summary>
    private static float HaggleFactor(PlayerData player, bool buying)
    {
        if (player == null) return 1f;

        int mod = DerivedStats.Mod(player.Charisma);
        float fromCharisma = mod * 0.02f;

        int fromTraits = 0;
        if (TraitSystem.Instance != null)
        {
            fromTraits = buying
                ? TraitSystem.Instance.GetPercentBonus(EffectType.ShopBuyPrice)
                : TraitSystem.Instance.GetPercentBonus(EffectType.ShopSellPrice);
        }

        float factor = buying
            ? 1f - fromCharisma + fromTraits / 100f
            : 1f + fromCharisma + fromTraits / 100f;

        return Mathf.Clamp(factor, 0.6f, 1.6f);
    }

    // -----------------------------------------------------------------

    private static bool IsMetal(ItemSO item)
    {
        string n = item.itemName.ToLowerInvariant();
        return n.Contains("ore") || n.Contains("ingot") || n.Contains("coal")
            || n.Contains("rivet") || n.Contains("buckle");
    }

    private static bool IsWood(ItemSO item)
    {
        string n = item.itemName.ToLowerInvariant();
        return n.Contains("log") || n.Contains("plank") || n.Contains("beam")
            || n.Contains("branch") || n.Contains("charcoal") || n.Contains("pitch");
    }

    private static bool IsAnimal(ItemSO item)
    {
        string n = item.itemName.ToLowerInvariant();
        return n.Contains("hide") || n.Contains("pelt") || n.Contains("wool")
            || n.Contains("leather") || n.Contains("sinew") || n.Contains("bone")
            || n.Contains("horn");
    }

    private static bool IsStone(ItemSO item)
    {
        string n = item.itemName.ToLowerInvariant();
        return n.Contains("stone") || n.Contains("clay") || n.Contains("brick")
            || n.Contains("limestone") || n.Contains("mortar");
    }

    /// <summary>
    /// Explains a price to the player. Shown in the tooltip so a high number
    /// reads as a reason rather than as the game being unfair.
    /// </summary>
    public static string ExplainPrice(ItemSO item, Shops shop, Settlement settlement, PlayerData player)
    {
        if (item == null || settlement == null) return "";

        float supply = SupplyFactor(item, settlement);
        float wealth = WealthFactor(settlement);

        if (supply <= 0.8f) return "Common here.";
        if (supply >= 1.3f) return "Scarce this far out.";
        if (wealth >= 1.15f) return "A wealthy market.";
        if (wealth <= 0.9f) return "Little coin in this place.";

        // Nothing about the town stands out, so say what this counter is doing.
        // A price the player cannot account for reads as noise.
        if (shop != null)
        {
            if (StockFactor(item, shop, buying: true) < 1f) return "They have plenty of these.";
            if (StockFactor(item, shop, buying: true) > 1f) return "Their last one.";
            if (TillFactor(shop, buying: true) < 1f)        return "This one needs the coin.";
            if (TillFactor(shop, buying: true) > 1f)        return "A well-kept shop.";
            if (shop.level >= 4)                            return "You pay for the address.";
        }

        return "";
    }
}
