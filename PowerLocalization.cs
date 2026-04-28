using System.Collections.Generic;
using System.Linq;

namespace LoudCrowMod;

internal static class PowerLocalization
{
    internal static readonly Dictionary<string, string> Entries = new()
    {
        ["LOUDCROWMOD-BULLET_MAGAZINE.title"] = "Bullet Magazine",
        ["LOUDCROWMOD-BULLET_MAGAZINE.description"] = $"Consume 1 {LoudCrowTextStyles.Bullet} per hit. Queue ({{Count}}): {{Queue}}.",
        ["LOUDCROWMOD-MAKESHIFT_MEASURE_FRAIL_POWER.title"] = "Shaky Guard",
        ["LOUDCROWMOD-MAKESHIFT_MEASURE_FRAIL_POWER.description"] = "At the start of your next turn, gain {Amount} Frail.",
        ["LOUDCROWMOD-MOST_WANTED_POWER.title"] = "Most Wanted",
        ["LOUDCROWMOD-MOST_WANTED_POWER.description"] = "Bullet effects on this target are doubled this turn.",
        ["LOUDCROWMOD-DULLNESS_POWER.title"] = "둔감",
        ["LOUDCROWMOD-DULLNESS_POWER.description"] = "취약/약화/손상의 효과가 절반으로 감소합니다.",
        ["LOUDCROWMOD-INTOXICATION_POWER.title"] = "도취",
        ["LOUDCROWMOD-INTOXICATION_POWER.description"] = $"Whenever you consume a {LoudCrowTextStyles.Bullet}, gain {{Amount}} temporary {LoudCrowTextStyles.Strength} this turn.",
        ["LOUDCROWMOD-HEAT_CIRCULATION_POWER.title"] = "Heat Circulation",
        ["LOUDCROWMOD-HEAT_CIRCULATION_POWER.description"] = $"Whenever you draw {LoudCrowTextStyles.Heat}, draw {{Amount}} card.",
        ["LOUDCROWMOD-IGNITION_ENGRAVING_POWER.title"] = "격발 각인",
        ["LOUDCROWMOD-IGNITION_ENGRAVING_POWER.description"] = "다음 탄환의 효과가 2배가 됩니다.",
        ["LOUDCROWMOD-MASTER_OF_CHAOS_POWER.title"] = "Master of Chaos",
        ["LOUDCROWMOD-MASTER_OF_CHAOS_POWER.description"] = $"Whenever you consume a {LoudCrowTextStyles.Bullet}, gain {{Amount}} {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-CAW_CAW_STRENGTH_POWER.title"] = "Caw Caw!",
        ["LOUDCROWMOD-CAW_CAW_STRENGTH_POWER.description"] = $"At the start of this creature's next turn, it gains {{Amount}} {LoudCrowTextStyles.Strength}.",
        ["LOUDCROWMOD-READY_TO_FIRE_POWER.title"] = "Ready to Fire",
        ["LOUDCROWMOD-READY_TO_FIRE_POWER.description"] = "Your next \"Shot\" card costs 0.",
        ["LOUDCROWMOD-CROW_FORM_POWER.title"] = "Crow Form",
        ["LOUDCROWMOD-CROW_FORM_POWER.description"] = "At the start of each turn, choose 1 of 3 cards from your Exhaust pile. It costs 0 this turn and is added to your hand.",
        ["LOUDCROWMOD-MAGIC_BULLET_MARKSMAN_POWER.title"] = "마탄의 사수",
        ["LOUDCROWMOD-MAGIC_BULLET_MARKSMAN_POWER.description"] = "내 턴 시작 시, 마탄을 {Amount} 얻습니다.",
        ["LOUDCROWMOD-JUSTICE_APOSTLE_POWER.title"] = "Justice Apostle",
        ["LOUDCROWMOD-JUSTICE_APOSTLE_POWER.description"] = $"Whenever you play an Attack this turn, gain 1 {LoudCrowTextStyles.Bullet}.",
        ["LOUDCROWMOD-BLACK_FEATHER_STRENGTH_LOSS_POWER.title"] = "Black Feather",
        ["LOUDCROWMOD-BLACK_FEATHER_STRENGTH_LOSS_POWER.description"] = "At the end of your turn, lose {Amount} Strength.",
        ["LOUDCROWMOD-CROW_FEATHER_STRENGTH_LOSS_POWER.title"] = "Crow Feather",
        ["LOUDCROWMOD-CROW_FEATHER_STRENGTH_LOSS_POWER.description"] = "At the end of your turn, lose {Amount} Strength.",
        ["LOUDCROWMOD-OUTLAW_POWER.title"] = "Outlaw",
        ["LOUDCROWMOD-OUTLAW_POWER.description"] = "This turn, all cards cost 1 less. Played cards Exhaust. At the end of turn, Exhaust your hand.",
        ["LOUDCROWMOD-SPEC_SPECIAL_10_POWER.title"] = "오기",
        ["LOUDCROWMOD-SPEC_SPECIAL_10_POWER.description"] = $"Whenever you gain a negative effect, gain {{Amount}} {LoudCrowTextStyles.Bullets}.",
        ["LOUDCROWMOD-PERSEVERANCE_POWER.title"] = "인내",
        ["LOUDCROWMOD-PERSEVERANCE_POWER.description"] = $"At the start of your turn, if you have a negative effect, gain {{Amount}} {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-SPEC_RARE_01_POWER.title"] = "열광",
        ["LOUDCROWMOD-SPEC_RARE_01_POWER.description"] = $"Whenever you create {LoudCrowTextStyles.Heat}, gain {{Amount}} Vigor.",
        ["LOUDCROWMOD-SPEC_SPECIAL_08_POWER.title"] = "열기 순환",
        ["LOUDCROWMOD-SPEC_SPECIAL_08_POWER.description"] = $"Whenever {LoudCrowTextStyles.Heat} Exhausts, gain {{Amount}} {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-SPEC_RARE_05_POWER.title"] = "예열",
        ["LOUDCROWMOD-SPEC_RARE_05_POWER.description"] = $"At the start of your turn, gain 1 Energy and add 1 {LoudCrowTextStyles.Heat} into your Discard Pile.",
        ["LOUDCROWMOD-LOADED_MAGIC_BULLET_POWER.title"] = "마탄",
        ["LOUDCROWMOD-LOADED_MAGIC_BULLET_POWER.description"] = "다음 마탄의 효과를 조건 없이 발동합니다.",
        ["LOUDCROWMOD-SPEC_RARE_06_POWER.title"] = "오발 장전",
        ["LOUDCROWMOD-SPEC_RARE_06_POWER.description"] = "At the start of your turn, load 1 Misfire.",
        ["LOUDCROWMOD-SPEC_RARE_02_POWER.title"] = "잔향 사격",
        ["LOUDCROWMOD-SPEC_RARE_02_POWER.description"] = "This turn, whenever you play another Attack, all copies in your Discard pile share this effect and are played on random enemies.",
        ["LOUDCROWMOD-AMMO_DEPOT_POWER.title"] = "Ammo Depot",
        ["LOUDCROWMOD-AMMO_DEPOT_POWER.description"] = $"At the start of each turn, gain {{Amount}} random {LoudCrowTextStyles.Bullets}.",
        ["LOUDCROWMOD-PERFECT_FINISH_POWER.title"] = "Perfect Finish",
        ["LOUDCROWMOD-PERFECT_FINISH_POWER.description"] = $"At the end of your turn, if your hand is empty, gain {{Amount}} {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-SPEC_RARE_10_POWER.title"] = "한발 더",
        ["LOUDCROWMOD-SPEC_RARE_10_POWER.description"] = "At the start of your turn, draw 1 card. It gains Exhaust.",
    };

    static PowerLocalization()
    {
        AddDefaultRuntimeAliases();

        AddRuntimeAliases("BULLET_MAGAZINE", "LOUDCROWMOD-BULLET_MAGAZINE");
        AddRuntimeAliases("MAKESHIFT_MEASURE_FRAIL_POWER", "LOUDCROWMOD-MAKESHIFT_MEASURE_FRAIL_POWER");
        AddRuntimeAliases("MOST_WANTED_POWER", "LOUDCROWMOD-MOST_WANTED_POWER");
        AddRuntimeAliases("DULLNESS_POWER", "LOUDCROWMOD-DULLNESS_POWER");
        AddRuntimeAliases("INTOXICATION_POWER", "LOUDCROWMOD-INTOXICATION_POWER");
        AddRuntimeAliases("HEAT_CIRCULATION_POWER", "LOUDCROWMOD-HEAT_CIRCULATION_POWER");
        AddRuntimeAliases("IGNITION_ENGRAVING_POWER", "LOUDCROWMOD-IGNITION_ENGRAVING_POWER");
        AddRuntimeAliases("MASTER_OF_CHAOS_POWER", "LOUDCROWMOD-MASTER_OF_CHAOS_POWER");
        AddRuntimeAliases("CAW_CAW_STRENGTH_POWER", "LOUDCROWMOD-CAW_CAW_STRENGTH_POWER");
        AddRuntimeAliases("READY_TO_FIRE_POWER", "LOUDCROWMOD-READY_TO_FIRE_POWER");
        AddRuntimeAliases("CROW_FORM_POWER", "LOUDCROWMOD-CROW_FORM_POWER");
        AddRuntimeAliases("MAGIC_BULLET_MARKSMAN_POWER", "LOUDCROWMOD-MAGIC_BULLET_MARKSMAN_POWER");
        AddRuntimeAliases("JUSTICE_APOSTLE_POWER", "LOUDCROWMOD-JUSTICE_APOSTLE_POWER");
        AddRuntimeAliases("BLACK_FEATHER_STRENGTH_LOSS_POWER", "LOUDCROWMOD-BLACK_FEATHER_STRENGTH_LOSS_POWER");
        AddRuntimeAliases("CROW_FEATHER_STRENGTH_LOSS_POWER", "LOUDCROWMOD-CROW_FEATHER_STRENGTH_LOSS_POWER");
        AddRuntimeAliases("OUTLAW_POWER", "LOUDCROWMOD-OUTLAW_POWER");
        AddRuntimeAliases("PERSEVERANCE_POWER", "LOUDCROWMOD-PERSEVERANCE_POWER");
        AddRuntimeAliases("LOADED_MAGIC_BULLET_POWER", "LOUDCROWMOD-LOADED_MAGIC_BULLET_POWER");
        AddRuntimeAliases("AMMO_DEPOT_POWER", "LOUDCROWMOD-AMMO_DEPOT_POWER");
        AddRuntimeAliases("PERFECT_FINISH_POWER", "LOUDCROWMOD-PERFECT_FINISH_POWER");

        AddRuntimeAliases("SPEC_SPECIAL10_POWER", "LOUDCROWMOD-SPEC_SPECIAL_10_POWER");
        AddRuntimeAliases("SPEC_SPECIAL08_POWER", "LOUDCROWMOD-SPEC_SPECIAL_08_POWER");
        AddRuntimeAliases("SPEC_RARE01_POWER", "LOUDCROWMOD-SPEC_RARE_01_POWER");
        AddRuntimeAliases("SPEC_RARE02_POWER", "LOUDCROWMOD-SPEC_RARE_02_POWER");
        AddRuntimeAliases("LOUDCROWMOD-SPEC_RARE02_POWER", "LOUDCROWMOD-SPEC_RARE_02_POWER");
        AddRuntimeAliases("SPEC_RARE05_POWER", "LOUDCROWMOD-SPEC_RARE_05_POWER");
        AddRuntimeAliases("SPEC_RARE06_POWER", "LOUDCROWMOD-SPEC_RARE_06_POWER");
        AddRuntimeAliases("SPEC_RARE10_POWER", "LOUDCROWMOD-SPEC_RARE_10_POWER");
    }

    private static void AddDefaultRuntimeAliases()
    {
        var source = Entries.ToArray();
        foreach (var pair in source)
        {
            if (!pair.Key.StartsWith("LOUDCROWMOD-"))
                continue;

            int suffixIndex = pair.Key.LastIndexOf('.');
            if (suffixIndex < 0)
                continue;

            string runtimeEntry = pair.Key["LOUDCROWMOD-".Length..suffixIndex];
            string suffix = pair.Key[suffixIndex..];
            Entries[$"{runtimeEntry}{suffix}"] = pair.Value;

            string normalized = NormalizeSpecRuntimeEntry(runtimeEntry);
            if (normalized != runtimeEntry)
                Entries[$"{normalized}{suffix}"] = pair.Value;
        }
    }

    private static string NormalizeSpecRuntimeEntry(string runtimeEntry)
    {
        if (runtimeEntry.StartsWith("SPEC_RARE_"))
            return runtimeEntry.Replace("SPEC_RARE_", "SPEC_RARE");
        if (runtimeEntry.StartsWith("SPEC_SPECIAL_"))
            return runtimeEntry.Replace("SPEC_SPECIAL_", "SPEC_SPECIAL");

        return runtimeEntry;
    }

    private static void AddRuntimeAliases(string runtimeEntry, string canonicalPrefix)
    {
        CopyIfPresent($"{canonicalPrefix}.title", $"{runtimeEntry}.title");
        CopyIfPresent($"{canonicalPrefix}.description", $"{runtimeEntry}.description");
    }

    private static void CopyIfPresent(string sourceKey, string aliasKey)
    {
        if (!Entries.TryGetValue(sourceKey, out var value))
            return;

        Entries[aliasKey] = value;
    }
}


