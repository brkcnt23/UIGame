using System.Collections.Generic;
using System.Linq;
using NEXUS.Utilities;

public class NpcRepository
{
    private readonly List<NpcData> npcs = new();

    public IReadOnlyList<NpcData> All => npcs;

    public void LoadFromSlot(int slot)
    {
        JSONDataHandler handler = new JSONDataHandler(slot);
        var wrapper = handler.LoadData<NpcDataWrapper>("npcs.json");

        npcs.Clear();
        if (wrapper?.npcs != null)
            npcs.AddRange(wrapper.npcs);
    }

    public void LoadFromSource()
    {
        JSONDataHandler handler = new JSONDataHandler("SourceData");
        var wrapper = handler.LoadData<NpcDataWrapper>("npcs.json");

        npcs.Clear();
        if (wrapper?.npcs != null)
            npcs.AddRange(wrapper.npcs);
    }

    public void SaveToSlot(int slot)
    {
        JSONDataHandler handler = new JSONDataHandler(slot);
        handler.SaveData(new NpcDataWrapper { npcs = npcs }, "npcs.json");
    }

    public NpcData GetById(string npcId)
    {
        return npcs.FirstOrDefault(x => x.NpcId == npcId);
    }

    public List<NpcData> GetBySettlement(int settlementId)
    {
        return npcs.Where(x => x.SettlementId == settlementId).ToList();
    }

    public List<NpcData> GetByShop(string shopId)
    {
        return npcs.Where(x => x.ShopBinding == shopId).ToList();
    }

    public List<NpcData> GetByRole(int settlementId, string roleTag)
    {
        return npcs
            .Where(x => x.SettlementId == settlementId && x.RoleTags.Contains(roleTag))
            .ToList();
    }
}