using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A pending trade, held until the player commits it.
///
/// Buying and selling one item at a time makes the player do arithmetic in
/// their head: "if I sell these three pelts, can I afford the ingots?" The
/// cart answers that question on screen — everything going each way, and one
/// number at the bottom that is either what you pay or what you take home.
///
/// Nothing moves until Confirm(). Until then the player can add, remove and
/// change their mind for free, which is what makes haggling feel like a
/// decision rather than a series of irreversible clicks.
/// </summary>
public sealed class TradeCart
{
    public sealed class Line
    {
        public int ItemId;
        public string Name;
        public Sprite Icon;
        public ItemQuality Quality;

        /// <summary>True when the player is buying this from the shop.</summary>
        public bool IsBuy;

        public int Quantity;

        /// <summary>Price per unit in silver, already fully modified.</summary>
        public int UnitSilver;

        public float UnitWeight;

        public int TotalSilver => UnitSilver * Quantity;
        public float TotalWeight => UnitWeight * Quantity;
    }

    private readonly List<Line> _lines = new();

    public IReadOnlyList<Line> Lines => _lines;
    public bool IsEmpty => _lines.Count == 0;

    /// <summary>Raised on any change so the panel can redraw once.</summary>
    public System.Action OnChanged;

    // -----------------------------------------------------------------
    // Totals
    // -----------------------------------------------------------------

    /// <summary>Silver leaving the player's purse.</summary>
    public int TotalCost
    {
        get
        {
            int sum = 0;
            foreach (var line in _lines)
                if (line.IsBuy) sum += line.TotalSilver;
            return sum;
        }
    }

    /// <summary>Silver coming in.</summary>
    public int TotalIncome
    {
        get
        {
            int sum = 0;
            foreach (var line in _lines)
                if (!line.IsBuy) sum += line.TotalSilver;
            return sum;
        }
    }

    /// <summary>
    /// Positive means the player walks away richer, negative means they pay.
    /// This is the single number the trade screen shows.
    /// </summary>
    public int NetSilver => TotalIncome - TotalCost;

    /// <summary>Weight change: bought items minus sold ones.</summary>
    public float NetWeight
    {
        get
        {
            float sum = 0f;
            foreach (var line in _lines)
                sum += line.IsBuy ? line.TotalWeight : -line.TotalWeight;
            return sum;
        }
    }

    /// <summary>"+12g 40s" or "-3g 15s", ready to display.</summary>
    public string NetLabel()
    {
        int net = NetSilver;
        string sign = net >= 0 ? "+" : "-";
        int abs = Mathf.Abs(net);

        int gold = abs / 100;
        int silver = abs % 100;

        return gold > 0 ? $"{sign}{gold}g {silver}s" : $"{sign}{silver}s";
    }

    public Color NetColor()
    {
        int net = NetSilver;
        if (net > 0) return new Color(0.56f, 0.77f, 0.42f);
        if (net < 0) return new Color(0.90f, 0.76f, 0.36f);
        return Color.white;
    }

    // -----------------------------------------------------------------
    // Editing
    // -----------------------------------------------------------------

    public Line Find(int itemId, bool isBuy)
        => _lines.Find(l => l.ItemId == itemId && l.IsBuy == isBuy);

    public void Add(int itemId, string name, Sprite icon, ItemQuality quality,
                    bool isBuy, int unitSilver, float unitWeight, int quantity = 1)
    {
        if (quantity <= 0) return;

        var existing = Find(itemId, isBuy);

        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            _lines.Add(new Line
            {
                ItemId = itemId,
                Name = name,
                Icon = icon,
                Quality = quality,
                IsBuy = isBuy,
                Quantity = quantity,
                UnitSilver = unitSilver,
                UnitWeight = unitWeight
            });
        }

        OnChanged?.Invoke();
    }

    public void Remove(int itemId, bool isBuy, int quantity = 1)
    {
        var line = Find(itemId, isBuy);
        if (line == null) return;

        line.Quantity -= quantity;
        if (line.Quantity <= 0)
            _lines.Remove(line);

        OnChanged?.Invoke();
    }

    public void RemoveLine(Line line)
    {
        if (line == null) return;

        _lines.Remove(line);
        OnChanged?.Invoke();
    }

    public void Clear()
    {
        if (_lines.Count == 0) return;

        _lines.Clear();
        OnChanged?.Invoke();
    }

    // -----------------------------------------------------------------
    // Validation
    // -----------------------------------------------------------------

    /// <summary>
    /// Whether the trade can go through, and why not if it cannot.
    ///
    /// Money is checked on the net figure, not on the purchases alone — a
    /// player selling enough to cover their basket should not be told they
    /// are broke.
    /// </summary>
    public bool CanConfirm(PlayerData player, out string reason)
    {
        reason = null;

        if (IsEmpty)
        {
            reason = "Nothing selected.";
            return false;
        }

        if (player == null)
        {
            reason = "No player data.";
            return false;
        }

        int net = NetSilver;

        if (net < 0)
        {
            var money = player.GetMoney();
            int purse = money.Gold * 100 + money.Silver;

            if (purse < -net)
            {
                int missing = -net - purse;
                reason = $"You are {FormatSilver(missing)} short.";
                return false;
            }
        }

        float weightAfter = player.GetCurrentInventoryWeight() + NetWeight;
        float capacity = player.GetCarryCapacity();

        // The overload band is the hard stop; below it the player is allowed
        // to make themselves slow if they judge it worth the profit.
        if (weightAfter > capacity * 1.25f)
        {
            reason = $"That is {Mathf.CeilToInt(weightAfter - capacity * 1.25f)} too heavy to carry out.";
            return false;
        }

        return true;
    }

    /// <summary>A warning that does not block the trade — shown in amber.</summary>
    public string GetWarning(PlayerData player)
    {
        if (player == null || IsEmpty) return null;

        float weightAfter = player.GetCurrentInventoryWeight() + NetWeight;
        float capacity = player.GetCarryCapacity();

        if (capacity <= 0f) return null;

        float ratio = weightAfter / capacity;

        if (ratio >= 1.0f) return "You will be overburdened.";
        if (ratio >= 0.75f) return "You will be slowed by the weight.";

        return null;
    }

    public static string FormatSilver(int silver)
    {
        int gold = silver / 100;
        int rest = silver % 100;
        return gold > 0 ? $"{gold}g {rest}s" : $"{rest}s";
    }
}
