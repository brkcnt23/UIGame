using System.Collections.Generic;

/// <summary>
/// Authored trait data, matched to the icons already sitting in
/// UI Elements/ProfilePanel/traits/250x250.
///
/// Descriptions are written the way the game speaks: what it feels like, not
/// what it multiplies. The numbers live in the effects list and are rendered
/// underneath as their own line.
///
/// Origins are chosen once at creation and shape early play. Personality
/// traits are earned and lost through choices. Familiarity comes from doing
/// the work. Conditions expire.
/// </summary>
public sealed class TraitDef
{
    public string Id;
    public string Name;
    public string IconName;
    public TraitKind Kind;
    public TraitTone Tone = TraitTone.Neutral;
    public string Description;
    public int DurationHours;
    public List<GameplayEffect> Effects = new();
    public List<string> Removes = new();
    public List<string> Tags = new();
}

public static class TraitCatalog
{
    private static List<TraitDef> _all;
    public static List<TraitDef> All => _all ??= Build();

    private static GameplayEffect E(EffectType t, int v, bool pct = false)
        => new GameplayEffect(t, v, pct);

    private static List<TraitDef> Build()
    {
        var list = new List<TraitDef>();

        void T(string id, string name, string icon, TraitKind kind, TraitTone tone,
               string desc, int hours, GameplayEffect[] effects,
               string[] removes = null, string[] tags = null)
        {
            list.Add(new TraitDef
            {
                Id = id, Name = name, IconName = icon,
                Kind = kind, Tone = tone, Description = desc,
                DurationHours = hours,
                Effects = new List<GameplayEffect>(effects),
                Removes = new List<string>(removes ?? new string[0]),
                Tags = new List<string>(tags ?? new string[0])
            });
        }

        // ============ ORIGINS — chosen at creation, never lost ============

        T("origin_farm", "Farm Raised", "FarmRaised", TraitKind.Origin, TraitTone.Neutral,
          "You know what a field asks of a body. Long days do not frighten you, and you can tell good grain from bad by the smell.",
          0, new[] { E(EffectType.Constitution, 1), E(EffectType.ExhaustionGain, -10, true) },
          null, new[] { "farm_raised", "commoner" });

        T("origin_forge", "Forge Raised", "ForgeRaised", TraitKind.Origin, TraitTone.Neutral,
          "You grew up in the heat and the hammering. Metal makes sense to your hands before it makes sense to your head.",
          0, new[] { E(EffectType.CraftQuality, 8, true), E(EffectType.Strength, 1) },
          null, new[] { "forge_raised", "smith_apprentice" });

        T("origin_woodcutter", "Woodcutter Raised", "WoodcutterRaised", TraitKind.Origin, TraitTone.Neutral,
          "An axe sat in your hands before a spoon did. You read grain and knots without thinking about it.",
          0, new[] { E(EffectType.Strength, 1), E(EffectType.CraftResourceCost, -8, true) },
          null, new[] { "woodcutter_raised", "carpenter_apprentice" });

        T("origin_mason", "Mason Raised", "MasonRaised", TraitKind.Origin, TraitTone.Neutral,
          "Stone taught you patience. You measure twice because your father would cuff you if you did not.",
          0, new[] { E(EffectType.Constitution, 1), E(EffectType.BuildTime, -10, true) },
          null, new[] { "mason_raised", "mason_apprentice" });

        T("origin_caravan", "Caravan Raised", "CaravanRaised", TraitKind.Origin, TraitTone.Neutral,
          "You were carried between towns before you could walk between them. Roads feel like home and strangers do not worry you.",
          0, new[] { E(EffectType.TravelTime, -10, true), E(EffectType.ShopBuyPrice, -5, true) },
          null, new[] { "caravan_raised", "traveler" });

        T("origin_street", "Street Raised", "StreetRaised", TraitKind.Origin, TraitTone.Neutral,
          "Nobody fed you unless you arranged it. You notice hands, exits, and who is watching.",
          0, new[] { E(EffectType.Dexterity, 1), E(EffectType.AmbushAvoidance, 10, true) },
          null, new[] { "street_raised", "streetwise" });

        T("origin_chapel", "Chapel Raised", "ChapelRaised", TraitKind.Origin, TraitTone.Neutral,
          "You were taught letters and mercy in the same room. People trust a voice that sounds like the one that comforted them.",
          0, new[] { E(EffectType.Charisma, 1), E(EffectType.Persuasion, 1) },
          null, new[] { "chapel_raised", "literate" });

        T("origin_camp", "Camp Raised", "CampRaised", TraitKind.Origin, TraitTone.Neutral,
          "Soldiers raised you between marches. You can sleep anywhere and you know what a drawn blade means.",
          0, new[] { E(EffectType.Attack, 1), E(EffectType.ExhaustionGain, -5, true) },
          null, new[] { "camp_raised", "soldier_kin" });

        // ============ PERSONALITY — earned and lost ============

        T("trait_ambitious", "Ambitious", "Ambitious", TraitKind.Personality, TraitTone.Neutral,
          "You want more than the life you were handed, and you are not embarrassed about it.",
          0, new[] { E(EffectType.JobReward, 8, true), E(EffectType.EventSuccess, -1) });

        T("trait_calm_mind", "Calm Mind", "CalmMind", TraitKind.Personality, TraitTone.Positive,
          "Things go wrong and your hands stay steady. People notice, and it steadies them too.",
          0, new[] { E(EffectType.EventSuccess, 2), E(EffectType.CraftQuality, 5, true) },
          new[] { "cond_shaken" });

        T("trait_proud", "Proud", "Proud", TraitKind.Personality, TraitTone.Neutral,
          "You do not bow easily. Some doors open for that; others close.",
          0, new[] { E(EffectType.Persuasion, 2), E(EffectType.ShopBuyPrice, 5, true) },
          null, new[] { "proud" });

        T("trait_honest", "Honest Nature", "HonestNature", TraitKind.Personality, TraitTone.Positive,
          "You say the true thing even when the useful thing is right there.",
          0, new[] { E(EffectType.ShopSellPrice, 6, true), E(EffectType.Persuasion, 1) },
          null, new[] { "honest" });

        T("trait_cold_pragmatist", "Cold Pragmatist", "ColdPragmatist", TraitKind.Personality, TraitTone.Neutral,
          "You count the cost before you count the feeling. It has kept you alive; it has also cost you friends.",
          0, new[] { E(EffectType.ShopBuyPrice, -8, true), E(EffectType.Persuasion, -1) },
          null, new[] { "pragmatic", "cold" });

        T("trait_hidden_mercy", "Hidden Mercy", "HiddenMercy", TraitKind.Personality, TraitTone.Positive,
          "You act harder than you are. People find this out later, usually when it matters.",
          0, new[] { E(EffectType.EventSuccess, 1) },
          null, new[] { "merciful" });

        T("trait_kind_unyielding", "Kind But Unyielding", "KindButUnyielding", TraitKind.Personality, TraitTone.Positive,
          "You will help anyone. You will not be moved once you have decided.",
          0, new[] { E(EffectType.Persuasion, 2) },
          null, new[] { "kind", "resolute" });

        T("trait_risk_seeker", "Risk Seeker", "RiskSeeker", TraitKind.Personality, TraitTone.Neutral,
          "The safe road bores you. You have been right often enough to keep believing it.",
          0, new[] { E(EffectType.CriticalChance, 3, true), E(EffectType.DamageTaken, 5, true) },
          null, new[] { "reckless" });

        T("trait_patient_worker", "Patient Worker", "PatientWorker", TraitKind.Personality, TraitTone.Positive,
          "You would rather do it once, properly, than twice in a hurry.",
          0, new[] { E(EffectType.CraftQuality, 10, true), E(EffectType.CraftSpeed, -5, true) },
          null, new[] { "patient" });

        T("trait_quick_study", "Quick Study", "QuickStudy", TraitKind.Personality, TraitTone.Positive,
          "You watch a thing done twice and your hands already have an opinion about it.",
          0, new[] { E(EffectType.SkillXpGain, 12, true) },
          null, new[] { "quick_study" });

        T("trait_trade_sense", "Trade Sense", "TradeSense", TraitKind.Personality, TraitTone.Positive,
          "You can hear when a price is a story rather than a number.",
          0, new[] { E(EffectType.ShopBuyPrice, -7, true), E(EffectType.ShopSellPrice, 7, true) },
          null, new[] { "merchant_minded" });

        T("trait_keen_eye", "Keen Eye For Value", "KeenEyeForValue", TraitKind.Personality, TraitTone.Positive,
          "You can tell what a thing is worth before the seller tells you.",
          0, new[] { E(EffectType.ShopSellPrice, 8, true) },
          null, new[] { "appraiser" });

        T("trait_common_folk_ease", "Common Folk Ease", "CommonFolkEase", TraitKind.Personality, TraitTone.Positive,
          "Farmers and porters talk to you like one of their own, because you are.",
          0, new[] { E(EffectType.Persuasion, 2), E(EffectType.JobReward, 5, true) },
          null, new[] { "commoner_friendly" });

        T("trait_iron_routine", "Iron Routine", "IronRoutine", TraitKind.Personality, TraitTone.Positive,
          "You eat, sleep and rise at the same hours no matter where you are. It costs you spontaneity and buys you endurance.",
          0, new[] { E(EffectType.ExhaustionGain, -12, true) },
          null, new[] { "disciplined" });

        T("trait_camp_discipline", "Camp Discipline", "CampDiscipline", TraitKind.Personality, TraitTone.Positive,
          "Your kit is packed the same way every night. You find things in the dark.",
          0, new[] { E(EffectType.TravelTime, -8, true), E(EffectType.RationConsumption, -5, true) },
          null, new[] { "disciplined" });

        T("trait_hunger_hardened", "Hunger Hardened", "HungerHardened", TraitKind.Personality, TraitTone.Neutral,
          "You have gone without before. Your body has stopped panicking about it.",
          0, new[] { E(EffectType.RationConsumption, -12, true), E(EffectType.MaxHealth, -5) },
          null, new[] { "hardened" });

        T("trait_scrapwise", "Scrapwise", "Scrapwise", TraitKind.Personality, TraitTone.Positive,
          "You have learned what can still be used after everyone else has called it rubbish.",
          0, new[] { E(EffectType.CraftResourceCost, -10, true) },
          null, new[] { "resourceful" });

        T("trait_sure_hands", "Sure Hands", "SureHands", TraitKind.Personality, TraitTone.Positive,
          "Fine work does not make you nervous. Your fingers do what you tell them.",
          0, new[] { E(EffectType.CraftQuality, 8, true), E(EffectType.Dexterity, 1) });

        T("trait_lightfooted", "Lightfooted", "Lightfooted", TraitKind.Personality, TraitTone.Positive,
          "You move without announcing it. Doors and undergrowth forgive you.",
          0, new[] { E(EffectType.AmbushAvoidance, 12, true), E(EffectType.Accuracy, 1) });

        T("trait_brooding", "Brooding Depth", "BroodingDepth", TraitKind.Personality, TraitTone.Neutral,
          "You turn things over long after others have put them down. It makes you slow company and good counsel.",
          0, new[] { E(EffectType.EventSuccess, 2), E(EffectType.Persuasion, -1) });

        // ============ FAMILIARITY — earned by working ============

        T("fam_forge", "Forge Familiar", "ForgeFamiliar", TraitKind.Familiarity, TraitTone.Positive,
          "Enough hours at the anvil that the heat no longer surprises you.",
          0, new[] { E(EffectType.CraftQuality, 6, true), E(EffectType.CraftSpeed, 6, true) },
          null, new[] { "forge_familiar" });

        T("fam_woodcraft", "Woodcraft Familiar", "WoodcraftFamiliar", TraitKind.Familiarity, TraitTone.Positive,
          "You have shaped enough timber to know where it will split before it does.",
          0, new[] { E(EffectType.CraftResourceCost, -8, true), E(EffectType.BuildTime, -6, true) },
          null, new[] { "woodcraft_familiar" });

        T("fam_stonecraft", "Stonecraft Familiar", "StonecraftFamiliar", TraitKind.Familiarity, TraitTone.Positive,
          "Stone has taught you where to strike and where to leave it alone.",
          0, new[] { E(EffectType.BuildTime, -10, true) },
          null, new[] { "stonecraft_familiar" });

        // ============ CONDITIONS — positive, temporary ============

        T("cond_well_rested", "Well Rested", "WellRested", TraitKind.Condition, TraitTone.Positive,
          "You slept properly and it shows in everything you do today.",
          12, new[] { E(EffectType.SkillXpGain, 10, true), E(EffectType.ExhaustionGain, -15, true) },
          new[] { "cond_fatigued", "cond_exhausted" });

        T("cond_well_fed", "Well Fed", "WellFed", TraitKind.Condition, TraitTone.Positive,
          "A full stomach and no reason to think about the next meal.",
          10, new[] { E(EffectType.Constitution, 1), E(EffectType.HealthRegen, 2) },
          new[] { "cond_hungry", "cond_starving", "cond_malnourished" });

        T("cond_nourished", "Nourished", "Nourished", TraitKind.Condition, TraitTone.Positive,
          "You have eaten well for days running. Your body has started trusting it.",
          48, new[] { E(EffectType.MaxHealth, 5), E(EffectType.HealthRegen, 1) },
          new[] { "cond_starving", "cond_malnourished" });

        T("cond_fresh_meal", "Fresh Meal", "Fresh Meal", TraitKind.Condition, TraitTone.Positive,
          "Hot food, properly cooked. A small thing that changes the whole day.",
          6, new[] { E(EffectType.HealthRegen, 3) },
          new[] { "cond_rotten_meal" });

        T("cond_warmed", "Warmed by Fire", "Warmed by Fire", TraitKind.Condition, TraitTone.Positive,
          "The cold has left your hands. You had not noticed how much of you it was holding.",
          8, new[] { E(EffectType.ExhaustionGain, -10, true) },
          new[] { "cond_chilled", "cond_frozen", "cond_soaked" });

        T("cond_inspired", "Inspired", "Inspired", TraitKind.Condition, TraitTone.Positive,
          "Something has caught in you and the work is going quickly.",
          8, new[] { E(EffectType.CraftSpeed, 10, true), E(EffectType.CraftQuality, 5, true) });

        T("cond_inspired_song", "Inspired by Song", "Inspired by Song", TraitKind.Condition, TraitTone.Positive,
          "A tune from the tavern is still going round your head and your step matches it.",
          6, new[] { E(EffectType.TravelTime, -10, true), E(EffectType.EventSuccess, 1) });

        T("cond_focused", "Focused", "Focused", TraitKind.Condition, TraitTone.Positive,
          "The noise has fallen away and there is only the next thing.",
          4, new[] { E(EffectType.Accuracy, 2), E(EffectType.CraftQuality, 8, true) },
          new[] { "cond_confused" });

        T("cond_energized", "Energized", "Energized", TraitKind.Condition, TraitTone.Positive,
          "You could keep going for hours and you rather want to.",
          6, new[] { E(EffectType.ExhaustionGain, -20, true) },
          new[] { "cond_fatigued" });

        T("cond_hopeful", "Hopeful", "Hopeful", TraitKind.Condition, TraitTone.Positive,
          "Something went right and you are letting yourself believe more might.",
          12, new[] { E(EffectType.EventSuccess, 1), E(EffectType.Persuasion, 1) },
          new[] { "cond_despairing" });

        T("cond_lucky", "Lucky", "Lucky", TraitKind.Condition, TraitTone.Positive,
          "Things keep landing your way. You know better than to say so out loud.",
          10, new[] { E(EffectType.EventSuccess, 2), E(EffectType.CriticalChance, 3, true) },
          new[] { "cond_unlucky" });

        T("cond_blessed", "Blessed", "Blessed", TraitKind.Condition, TraitTone.Positive,
          "A priest laid a hand on you and something about the day has felt lighter since.",
          24, new[] { E(EffectType.IllnessResistance, 3), E(EffectType.EventSuccess, 1) },
          new[] { "cond_cursed" });

        T("cond_regenerating", "Regenerating", "Regenerating", TraitKind.Condition, TraitTone.Positive,
          "The wound is closing cleanly. Leave it alone and it will keep doing so.",
          12, new[] { E(EffectType.HealthRegen, 5) },
          new[] { "cond_bleeding" });

        T("cond_recovering", "Recovering", "Recovering", TraitKind.Condition, TraitTone.Positive,
          "Past the worst of it. Still not what you were.",
          24, new[] { E(EffectType.HealthRegen, 3), E(EffectType.Attack, -1) },
          new[] { "cond_diseased", "cond_feverish" });

        T("cond_adrenaline", "Adrenaline Rush", "Adrenaline Rush", TraitKind.Condition, TraitTone.Positive,
          "Your heart is going and you cannot feel the cut yet. You will later.",
          2, new[] { E(EffectType.Attack, 2), E(EffectType.DamageTaken, -10, true) });

        T("cond_iron_will", "Iron Will", "Iron Will", TraitKind.Condition, TraitTone.Positive,
          "You have decided you are not stopping, and the decision is holding.",
          6, new[] { E(EffectType.Defense, 2), E(EffectType.ExhaustionGain, -15, true) },
          new[] { "cond_terrified", "cond_shaken" });

        T("cond_bloodied_standing", "Bloodied But Standing", "Bloodied But Standing", TraitKind.Condition, TraitTone.Positive,
          "You took worse than you gave and you are still on your feet. Something in you has changed shape.",
          24, new[] { E(EffectType.Defense, 1), E(EffectType.EventSuccess, 1) });

        T("cond_guarded_stance", "Guarded Stance", "Guarded Stance", TraitKind.Condition, TraitTone.Positive,
          "Weight back, blade high. You are giving up reach for time.",
          3, new[] { E(EffectType.Defense, 3), E(EffectType.Attack, -1) },
          new[] { "cond_broken_guard" });

        T("cond_duelist_focus", "Duelist's Focus", "Duelist’s Focus", TraitKind.Condition, TraitTone.Positive,
          "One opponent, nothing else. You are reading them now.",
          3, new[] { E(EffectType.Accuracy, 3), E(EffectType.CriticalChance, 5, true) });

        T("cond_fearless_charge", "Fearless Charge", "Fearless Charge", TraitKind.Condition, TraitTone.Positive,
          "You went forward when going forward was the worse idea, and it worked.",
          2, new[] { E(EffectType.Attack, 3), E(EffectType.Defense, -2) });

        T("cond_steady_fire", "Steady Under Fire", "SteadyUnderFire", TraitKind.Condition, TraitTone.Positive,
          "Arrows are coming and your hands have not started shaking.",
          4, new[] { E(EffectType.Accuracy, 2), E(EffectType.Defense, 1) },
          new[] { "cond_terrified" });

        T("cond_shielded", "Shielded", "Shielded", TraitKind.Condition, TraitTone.Positive,
          "Someone is covering your side. You can spend attention elsewhere.",
          3, new[] { E(EffectType.Defense, 3) });

        T("cond_bloodlust", "Bloodlust", "Bloodlust", TraitKind.Condition, TraitTone.Neutral,
          "Something has come loose in you. You are hitting harder and thinking less.",
          3, new[] { E(EffectType.Attack, 3), E(EffectType.Defense, -2), E(EffectType.Persuasion, -2) });

        T("cond_enraged", "Enraged", "Enraged", TraitKind.Condition, TraitTone.Neutral,
          "You are past reasoning with, including by yourself.",
          2, new[] { E(EffectType.Attack, 4), E(EffectType.Accuracy, -2), E(EffectType.Defense, -2) });

        T("cond_vengeful", "Vengeful", "Vengeful", TraitKind.Condition, TraitTone.Neutral,
          "You are keeping a name in your head and it is making you sharper and worse.",
          48, new[] { E(EffectType.Attack, 2), E(EffectType.Persuasion, -2) });

        // ============ CONDITIONS — negative, temporary ============

        T("cond_hungry", "Hungry", "Hungry", TraitKind.Condition, TraitTone.Negative,
          "It has been too long since you ate and it is starting to take your attention.",
          0, new[] { E(EffectType.Constitution, -1), E(EffectType.ExhaustionGain, 10, true) },
          new[] { "cond_well_fed" });

        T("cond_starving", "Starving", "Starving", TraitKind.Condition, TraitTone.Negative,
          "Your body has begun taking what it needs from itself.",
          0, new[] { E(EffectType.Constitution, -2), E(EffectType.Strength, -1), E(EffectType.HealthRegen, -5) },
          new[] { "cond_well_fed", "cond_nourished" });

        T("cond_malnourished", "Malnourished", "Malnourished", TraitKind.Condition, TraitTone.Negative,
          "Weeks of poor eating. You bruise easily and you tire before you should.",
          0, new[] { E(EffectType.MaxHealth, -10), E(EffectType.ExhaustionGain, 15, true) },
          new[] { "cond_nourished" });

        T("cond_dehydrated", "Dehydrated", "Dehydrated", TraitKind.Condition, TraitTone.Negative,
          "Your head aches and the road looks longer than it is.",
          0, new[] { E(EffectType.ExhaustionGain, 20, true), E(EffectType.Accuracy, -1) });

        T("cond_fatigued", "Fatigued", "Fatigued", TraitKind.Condition, TraitTone.Negative,
          "Not dangerous yet. Just heavy.",
          0, new[] { E(EffectType.ExhaustionGain, 10, true), E(EffectType.CraftQuality, -5, true) },
          new[] { "cond_well_rested", "cond_energized" });

        T("cond_exhausted", "Exhausted", "Exhausted", TraitKind.Condition, TraitTone.Negative,
          "You are running on nothing and your judgement has gone with it.",
          0, new[] { E(EffectType.Attack, -2), E(EffectType.Defense, -2), E(EffectType.EventSuccess, -2) },
          new[] { "cond_well_rested" });

        T("cond_wounded", "Wounded", "Wounded", TraitKind.Condition, TraitTone.Negative,
          "Something is torn that should not be. It pulls when you move.",
          0, new[] { E(EffectType.MaxHealth, -15), E(EffectType.Attack, -1) });

        T("cond_bleeding", "Bleeding", "Bleeding", TraitKind.Condition, TraitTone.Negative,
          "It has not stopped on its own and it is not going to.",
          6, new[] { E(EffectType.HealthRegen, -8) },
          new[] { "cond_regenerating" });

        T("cond_cracked_ribs", "Cracked Ribs", "Cracked Ribs", TraitKind.Condition, TraitTone.Negative,
          "Breathing has become a decision you make carefully.",
          72, new[] { E(EffectType.Constitution, -2), E(EffectType.ExhaustionGain, 15, true) });

        T("cond_broken_guard", "Broken Guard", "Broken Guard", TraitKind.Condition, TraitTone.Negative,
          "Your arm was knocked wide and you have not recovered the line.",
          2, new[] { E(EffectType.Defense, -3) },
          new[] { "cond_guarded_stance" });

        T("cond_poisoned", "Poisoned", "Poisoned", TraitKind.Condition, TraitTone.Negative,
          "Something is in you that should not be, and it is patient.",
          12, new[] { E(EffectType.HealthRegen, -6), E(EffectType.Strength, -1) });

        T("cond_diseased", "Diseased", "Diseased", TraitKind.Condition, TraitTone.Negative,
          "It came on slowly and it will leave the same way.",
          72, new[] { E(EffectType.Constitution, -2), E(EffectType.MaxHealth, -10) });

        T("cond_feverish", "Feverish", "Feverish", TraitKind.Condition, TraitTone.Negative,
          "Too hot, then too cold. The road keeps moving when you stand still.",
          24, new[] { E(EffectType.Accuracy, -2), E(EffectType.EventSuccess, -1) });

        T("cond_burning", "Burning", "Burning", TraitKind.Condition, TraitTone.Negative,
          "The fire has caught and beating at it is not working.",
          2, new[] { E(EffectType.HealthRegen, -12), E(EffectType.Defense, -1) });

        T("cond_chilled", "Chilled", "Chilled", TraitKind.Condition, TraitTone.Negative,
          "The cold has got past your clothes and settled.",
          6, new[] { E(EffectType.Dexterity, -1), E(EffectType.ExhaustionGain, 10, true) },
          new[] { "cond_warmed" });

        T("cond_frozen", "Frozen", "Frozen", TraitKind.Condition, TraitTone.Negative,
          "Your fingers have stopped answering. You cannot feel the strap you are pulling.",
          4, new[] { E(EffectType.Dexterity, -3), E(EffectType.Accuracy, -2) },
          new[] { "cond_warmed" });

        T("cond_soaked", "Soaked", "Soaked", TraitKind.Condition, TraitTone.Negative,
          "Wet through, and the wind has found you.",
          5, new[] { E(EffectType.ExhaustionGain, 15, true) },
          new[] { "cond_warmed" });

        // Three weight tiers share one icon. The severity is in the numbers and
        // the wording, not in three separate drawings — the same principle that
        // keeps the chest from needing small, medium and large artwork.
        T("cond_burdened", "Burdened", "Overburdened", TraitKind.Condition, TraitTone.Negative,
          "The pack has started to argue with you on hills.",
          0, new[] { E(EffectType.TravelTime, 15, true), E(EffectType.ExhaustionGain, 15, true) },
          new[] { "cond_overburdened", "cond_overloaded" });

        T("cond_overburdened", "Overburdened", "Overburdened", TraitKind.Condition, TraitTone.Negative,
          "You are carrying more than sense allows and every step says so.",
          0, new[] { E(EffectType.TravelTime, 40, true), E(EffectType.ExhaustionGain, 40, true),
                     E(EffectType.Defense, -1), E(EffectType.AmbushAvoidance, -20, true) },
          new[] { "cond_burdened", "cond_overloaded" });

        T("cond_overloaded", "Overloaded", "Overburdened", TraitKind.Condition, TraitTone.Negative,
          "You cannot walk with this. Something has to be left behind.",
          0, new[] { E(EffectType.TravelTime, 100, true), E(EffectType.ExhaustionGain, 80, true),
                     E(EffectType.Defense, -3), E(EffectType.Accuracy, -2) },
          new[] { "cond_burdened", "cond_overburdened" });

        T("cond_stunned", "Stunned", "Stunned", TraitKind.Condition, TraitTone.Negative,
          "The world has gone flat and loud and you have lost a moment somewhere.",
          1, new[] { E(EffectType.Defense, -3), E(EffectType.Accuracy, -3) });

        T("cond_confused", "Confused", "Confused", TraitKind.Condition, TraitTone.Negative,
          "You keep reaching for the wrong thing.",
          3, new[] { E(EffectType.Accuracy, -2), E(EffectType.CraftQuality, -10, true) },
          new[] { "cond_focused" });

        T("cond_shaken", "Shaken", "Shaken", TraitKind.Condition, TraitTone.Negative,
          "Your hands have not settled since it happened.",
          8, new[] { E(EffectType.Accuracy, -2), E(EffectType.CraftQuality, -8, true) },
          new[] { "cond_calm", "cond_iron_will" });

        T("cond_terrified", "Terrified", "Terrified", TraitKind.Condition, TraitTone.Negative,
          "Everything in you wants to be somewhere else.",
          4, new[] { E(EffectType.Attack, -3), E(EffectType.EventSuccess, -2) },
          new[] { "cond_iron_will", "cond_steady_fire" });

        T("cond_despairing", "Despairing", "Despairing", TraitKind.Condition, TraitTone.Negative,
          "You cannot find the reason to do the next thing.",
          24, new[] { E(EffectType.EventSuccess, -2), E(EffectType.SkillXpGain, -15, true) },
          new[] { "cond_hopeful" });

        T("cond_traumatized", "Traumatized", "Traumatized", TraitKind.Condition, TraitTone.Negative,
          "It comes back at the wrong times and takes your attention with it.",
          120, new[] { E(EffectType.EventSuccess, -1), E(EffectType.ExhaustionGain, 10, true) });

        T("cond_haunted_dreams", "Haunted Dreams", "Haunted Dreams", TraitKind.Condition, TraitTone.Negative,
          "Sleep is no longer rest. You wake more tired than you lay down.",
          48, new[] { E(EffectType.ExhaustionGain, 20, true) },
          new[] { "cond_well_rested" });

        T("cond_guilty", "Guilty Conscience", "Guilty Conscience", TraitKind.Condition, TraitTone.Negative,
          "You did the thing that worked. You keep going back to it anyway.",
          72, new[] { E(EffectType.Persuasion, -2), E(EffectType.EventSuccess, -1) });

        T("cond_unlucky", "Unlucky", "Unlucky", TraitKind.Condition, TraitTone.Negative,
          "Nothing has gone catastrophically wrong. Everything has gone slightly wrong.",
          10, new[] { E(EffectType.EventSuccess, -2) },
          new[] { "cond_lucky" });

        T("cond_cursed", "Cursed", "Cursed", TraitKind.Condition, TraitTone.Negative,
          "An old woman said something over you and you have not been able to forget it.",
          72, new[] { E(EffectType.EventSuccess, -2), E(EffectType.IllnessResistance, -3) },
          new[] { "cond_blessed" });

        T("cond_marked", "Marked for Death", "Marked for Death", TraitKind.Condition, TraitTone.Negative,
          "Someone has paid for your name. You do not know who yet.",
          168, new[] { E(EffectType.DamageTaken, 15, true), E(EffectType.AmbushAvoidance, -15, true) });

        T("cond_silenced", "Silenced", "Silenced", TraitKind.Condition, TraitTone.Negative,
          "You cannot talk your way out of this one.",
          3, new[] { E(EffectType.Persuasion, -4) });

        T("cond_rotten_meal", "Rotten Meal", "Rotten Meal", TraitKind.Condition, TraitTone.Negative,
          "It looked fine. It was not fine.",
          8, new[] { E(EffectType.Constitution, -1), E(EffectType.HealthRegen, -3) },
          new[] { "cond_fresh_meal" });

        return list;
    }
}
