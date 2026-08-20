using UnityEngine;

/// <summary>
/// Executes what the cart describes.
///
/// The cart is a proposal; this is the only thing that moves goods and coin,
/// and it does so in one step. Half-applied trades — money taken but items
/// not delivered because the last line failed a check — are the classic
/// shop bug, so everything is validated before anything changes.
/// </summary>
public sealed class TradeSystem : GameSystemBase
{
    public override int Priority => SystemPriority.Shop + 5;

    public static TradeSystem Instance { get; private set; }

    [SerializeField] private bool verbose;

    /// <summary>The cart the shop screen is currently editing.</summary>
    public TradeCart Cart { get; private set; } = new TradeCart();

    /// <summary>Raised after a successful trade, with the net silver moved.</summary>
    public System.Action<int> OnTradeCompleted;

    private Shops _shop;
    private Settlement _settlement;

    protected override void OnInitialize()
    {
        Instance = this;
    }

    protected override void OnShutdown()
    {
        if (Instance == this) Instance = null;
    }

    // -----------------------------------------------------------------

    /// <summary>Called when the player enters a shop.</summary>
    public void OpenShop(Shops shop, Settlement settlement)
    {
        _shop = shop;
        _settlement = settlement;
        Cart = new TradeCart();

        if (verbose) Log($"Opened {shop?.Name} in {settlement?.Name}.");
    }

    public void CloseShop()
    {
        Cart.Clear();
        _shop = null;
        _settlement = null;
    }

    // -----------------------------------------------------------------
    // Price lookups the UI calls per row
    // -----------------------------------------------------------------

    public int GetBuyPrice(ItemSO item)
        => PricingSystem.GetBuyPrice(item, _shop, _settlement, Player);

    public int GetSellPrice(ItemSO item)
        => PricingSystem.GetSellPrice(item, _shop, _settlement, Player);

    public string ExplainPrice(ItemSO item)
        => PricingSystem.ExplainPrice(item, _shop, _settlement, Player);

    /// <summary>Adds one unit of a shop item to the buy side.</summary>
    public void AddToBuy(ItemSO item, int quantity = 1)
    {
        if (item == null) return;

        Cart.Add(item.ID, item.itemName, item.icon, item.quality,
                 isBuy: true, GetBuyPrice(item), item.weight, quantity);
    }

    /// <summary>Adds one unit of a carried item to the sell side.</summary>
    public void AddToSell(Item carried, int quantity = 1)
    {
        if (carried == null) return;

        var template = LookupTemplate(carried.ID);
        if (template == null) return;

        if (!ShopAccepts(template))
        {
            if (verbose) Log($"{_shop?.Name} does not deal in {template.itemName}.");
            return;
        }

        int unit = PricingSystem.GetSellPrice(carried, template, _shop, _settlement, Player);

        Cart.Add(carried.ID, carried.Name, carried.ItemImage,
                 (ItemQuality)Mathf.Clamp(carried.Quality, 0, 4),
                 isBuy: false, unit, carried.UnitWeight, quantity);
    }

    public bool ShopAccepts(ItemSO item)
    {
        if (item == null) return false;
        if (_shop?.AcceptedCategories == null || _shop.AcceptedCategories.Count == 0) return true;

        return _shop.AcceptedCategories.Contains(item.category);
    }

    // -----------------------------------------------------------------
    // Commit
    // -----------------------------------------------------------------

    /// <summary>
    /// Applies the whole cart or none of it. Returns false with a reason when
    /// the trade is refused.
    /// </summary>
    public bool Confirm(out string reason)
    {
        var player = Player;

        if (!Cart.CanConfirm(player, out reason))
            return false;

        int net = Cart.NetSilver;

        // Money first: if this fails the inventory has not been touched yet.
        if (net < 0 && !player.TrySpendMoney(0, -net))
        {
            reason = "You cannot afford that.";
            return false;
        }

        if (net > 0)
            player.AddMoney(0, net);

        foreach (var line in Cart.Lines)
        {
            if (line.IsBuy) GiveToPlayer(line);
            else TakeFromPlayer(line);
        }

        if (verbose)
            Log($"Trade completed. Net {Cart.NetLabel()}, {Cart.Lines.Count} line(s).");

        int moved = net;
        Cart.Clear();

        OnTradeCompleted?.Invoke(moved);
        EncumbranceSystem.Instance?.Evaluate();
        PlayerStatHandler.Instance?.RefreshPlayerUI();

        reason = null;
        return true;
    }

    private void GiveToPlayer(TradeCart.Line line)
    {
        var template = LookupTemplate(line.ItemId);
        if (template == null) return;

        var player = Player;
        if (player?.Items == null) return;

        // Shop goods are ordinary work — Crude or Common. Anything better is
        // crafted or found, which is what keeps the forge worth using.
        var quality = template.quality > ItemQuality.Common
            ? ItemQuality.Common
            : template.quality;

        for (int i = 0; i < line.Quantity; i++)
        {
            var instance = template.RollInstance(quality);
            if (instance != null)
                player.Items.Add(instance);
        }

        Events?.Dispatch(new ItemAddedEvent(line.ItemId, line.Quantity));
    }

    private void TakeFromPlayer(TradeCart.Line line)
    {
        var player = Player;
        if (player?.Items == null) return;

        int remaining = line.Quantity;

        for (int i = player.Items.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var item = player.Items[i];
            if (item == null || item.ID != line.ItemId) continue;

            // Never sell what the player is wearing by accident.
            if (item.IsEquipped) continue;

            int take = Mathf.Min(remaining, Mathf.Max(1, item.Quantity));
            item.Quantity -= take;
            remaining -= take;

            if (item.Quantity <= 0)
                player.Items.RemoveAt(i);
        }

        Events?.Dispatch(new ItemRemovedEvent(line.ItemId, line.Quantity));
    }

    private ItemSO LookupTemplate(int itemId)
    {
        var db = Resources != null ? Resources.GetItemDatabase() : null;
        return db != null ? db.GetByID(itemId) : null;
    }

    private PlayerData Player => PlayerStatHandler.Instance?.pd;
}
