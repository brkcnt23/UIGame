using UnityEngine;

/// <summary>
/// Haritadaki settlement pin görselleri.
/// Tier başına kilitli / açık iki varyant.
/// Tek asset — sprite'ları buraya sürükle, harita otomatik kullanır.
/// </summary>
[CreateAssetMenu(fileName = "SettlementIconSet", menuName = "UIGame/Settlement Icon Set")]
public class SettlementIconSetSO : ScriptableObject
{
    [System.Serializable]
    public class TierIcons
    {
        public SettlementTier tier;
        public Sprite unlocked;
        public Sprite locked;      // boşsa unlocked gri tonlanır
    }

    public TierIcons[] tiers = new TierIcons[]
    {
        new TierIcons { tier = SettlementTier.Hamlet  },
        new TierIcons { tier = SettlementTier.Village },
        new TierIcons { tier = SettlementTier.Town    },
        new TierIcons { tier = SettlementTier.City    },
    };

    [Header("Özel")]
    [Tooltip("Oyuncunun doğduğu köy — diğerlerinden ayırt edilsin.")]
    public Sprite homeSettlement;
    [Tooltip("Quest ile açılan geçici konumlar.")]
    public Sprite questLocation;

    [Header("Pin çerçevesi")]
    [Tooltip("İsimlik / nameplate arka planı. İsim TMP ile üstüne yazılır.")]
    public Sprite nameplate;

    public Sprite GetIcon(SettlementTier tier, bool isUnlocked)
    {
        foreach (var t in tiers)
        {
            if (t.tier != tier) continue;
            if (isUnlocked) return t.unlocked;
            return t.locked != null ? t.locked : t.unlocked;
        }
        return null;
    }
}
