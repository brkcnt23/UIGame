using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ünvanın ait olduğu dal.
/// Milestone = her iki dalın buluştuğu ortak kilometre taşı.
/// </summary>
public enum TitleTrack
{
    Administrative,   // Standing ile ilerler
    Martial,          // Renown ile ilerler
    Milestone         // Ortak eşik: Reeve / Bailiff / Baron / Duke
}

/// <summary>
/// Tek bir ünvanın tanımı. Veri odaklı — yeni ünvan eklemek
/// bu listeye satır eklemekten ibarettir.
/// </summary>
[System.Serializable]
public class TitleDefinition
{
    [Header("Kimlik")]
    public string titleId;              // "reeve", "knight", "chamberlain"
    public string displayName;          // "Reeve"
    public TitleTrack track;
    [Tooltip("Kendi dalı içindeki sıra. Milestone'lar için 0.")]
    public int rankInTrack;
    [Tooltip("Milestone'lar için 1-4, diğerleri için hangi eşikten önce geldiği.")]
    public int segment;

    [Header("Görseller")]
    [Tooltip("Haritada oyuncu avatarının ikonu.")]
    public Sprite mapAvatarIcon;
    [Tooltip("Profil panelindeki ünvan rozeti.")]
    public Sprite titleBadge;

    [Header("Kazanma koşulu")]
    [Tooltip("Administrative ise Standing, Martial ise Renown eşiği.")]
    public int requiredReputation;
    [Tooltip("Sadece milestone'larda dolu. Boşsa quest gerekmez.")]
    public string ceremonyQuestId;

    [Header("Milestone metrik kapısı (sadece milestone'larda)")]
    public int requiredPopulation;
    public int requiredWealth;
    public int requiredQuality;

    [Header("Açtıkları")]
    public SettlementTier maxSettlementTier = SettlementTier.None;
    public int companionSlots;
    public int maxArmySize;
    public List<string> unlockTags = new();

    [Header("Anlatı")]
    [TextArea(2, 4)]
    public string flavorText;
}

public enum SettlementTier
{
    None,
    Hamlet,
    Village,
    Town,
    City
}

/// <summary>
/// Tüm ünvanların tek kaynağı. Tek asset — 26 satır tek Inspector'da,
/// sprite'ları listeden aşağı doğru sürükleyerek atarsın.
///
/// Sağ tık > "Populate Default Titles" ile 26 satır isimleriyle otomatik dolar.
/// </summary>
[CreateAssetMenu(fileName = "TitleDatabase", menuName = "UIGame/Title Database")]
public class TitleDatabaseSO : ScriptableObject
{
    [Tooltip("Kilometre taşı geçiş kuralı: toplam N ünvan, her daldan en az M.")]
    public int[] segmentTotalRequired = { 4, 6, 6, 6 };
    public int[] segmentMinPerTrack = { 1, 2, 2, 2 };

    public List<TitleDefinition> titles = new();

    // ---------------------------------------------------------------
    // Sorgular
    // ---------------------------------------------------------------

    public TitleDefinition GetById(string id)
    {
        return titles.Find(t => t.titleId == id);
    }

    public List<TitleDefinition> GetTrack(TitleTrack track)
    {
        return titles.FindAll(t => t.track == track);
    }

    public List<TitleDefinition> GetMilestones()
    {
        return titles.FindAll(t => t.track == TitleTrack.Milestone);
    }

    /// <summary>
    /// Oyuncunun bir sonraki kilometre taşına hak kazanıp kazanmadığı.
    /// segmentIndex: 0 = Reeve, 1 = Bailiff, 2 = Baron, 3 = Duke
    /// </summary>
    public bool MeetsSegmentRequirement(int segmentIndex, int adminTitlesEarned, int martialTitlesEarned)
    {
        if (segmentIndex < 0 || segmentIndex >= segmentTotalRequired.Length)
            return false;

        int total = adminTitlesEarned + martialTitlesEarned;
        int min = segmentMinPerTrack[segmentIndex];

        return total >= segmentTotalRequired[segmentIndex]
            && adminTitlesEarned >= min
            && martialTitlesEarned >= min;
    }

    // ---------------------------------------------------------------
    // Editör yardımcısı
    // ---------------------------------------------------------------

    [ContextMenu("Populate Default Titles")]
    private void PopulateDefaultTitles()
    {
        titles.Clear();

        // --- Segment 1: Reeve öncesi (2 + 2) ---
        Add("freeman",      "Freeman",          TitleTrack.Administrative, 1, 1);
        Add("tithingman",   "Tithingman",       TitleTrack.Administrative, 2, 1);
        Add("footman",      "Footman",          TitleTrack.Martial,        1, 1);
        Add("man_at_arms",  "Man-at-Arms",      TitleTrack.Martial,        2, 1);

        AddMilestone("reeve", "Reeve", 1, SettlementTier.Hamlet, 1);

        // --- Segment 2: Bailiff öncesi (3 + 3) ---
        Add("hayward",      "Hayward",          TitleTrack.Administrative, 3, 2);
        Add("beadle",       "Beadle",           TitleTrack.Administrative, 4, 2);
        Add("constable",    "Constable",        TitleTrack.Administrative, 5, 2);
        Add("veteran",      "Veteran",          TitleTrack.Martial,        3, 2);
        Add("sergeant",     "Sergeant",         TitleTrack.Martial,        4, 2);
        Add("squire",       "Squire",           TitleTrack.Martial,        5, 2);

        AddMilestone("bailiff", "Bailiff", 2, SettlementTier.Village, 2);

        // --- Segment 3: Baron öncesi (3 + 3) ---
        Add("warden",       "Warden",           TitleTrack.Administrative, 6, 3);
        Add("provost",      "Provost",          TitleTrack.Administrative, 7, 3);
        Add("chamberlain",  "Chamberlain",      TitleTrack.Administrative, 8, 3);
        Add("bannerman",    "Bannerman",        TitleTrack.Martial,        6, 3);
        Add("household_knight", "Household Knight", TitleTrack.Martial,    7, 3);
        Add("knight",       "Knight",           TitleTrack.Martial,        8, 3);

        AddMilestone("baron", "Baron", 3, SettlementTier.Town, 3);

        // --- Segment 4: Duke öncesi (3 + 3) ---
        Add("steward",      "Steward",          TitleTrack.Administrative, 9,  4);
        Add("seneschal",    "Seneschal",        TitleTrack.Administrative, 10, 4);
        Add("justiciar",    "Justiciar",        TitleTrack.Administrative, 11, 4);
        Add("knight_banneret", "Knight Banneret", TitleTrack.Martial,      9,  4);
        Add("castellan",    "Castellan",        TitleTrack.Martial,        10, 4);
        Add("marshal",      "Marshal",          TitleTrack.Martial,        11, 4);

        AddMilestone("duke", "Duke", 4, SettlementTier.City, 4);

        Debug.Log($"[TitleDatabase] {titles.Count} ünvan oluşturuldu.");
    }

    private void Add(string id, string name, TitleTrack track, int rank, int segment)
    {
        titles.Add(new TitleDefinition
        {
            titleId = id,
            displayName = name,
            track = track,
            rankInTrack = rank,
            segment = segment,
            requiredReputation = rank * 100,   // taslak eşik, dengelenecek
        });
    }

    private void AddMilestone(string id, string name, int segment, SettlementTier tier, int slots)
    {
        titles.Add(new TitleDefinition
        {
            titleId = id,
            displayName = name,
            track = TitleTrack.Milestone,
            rankInTrack = 0,
            segment = segment,
            maxSettlementTier = tier,
            companionSlots = slots,
            ceremonyQuestId = $"ceremony_{id}",
            maxArmySize = segment * 25,        // taslak, dengelenecek
        });
    }
}
