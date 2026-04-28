using Godot;
using HarmonyLib;
using LoudCrowMod.Cards;
using LoudCrowMod.Powers;
using LoudCrowMod.Relics;
using BaseLib.Abstracts;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Relics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace LoudCrowMod;

[ModInitializer(nameof(Initialize))]
public static class LoudCrowMod
{
    public const string ModId = "LoudCrowMod";

    public static void Initialize()
    {
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(
            System.Reflection.Assembly.GetExecutingAssembly());

        var harmony = new Harmony(ModId);
        harmony.PatchAll();
    }
}

internal static class LoudCrowPowerAssetPaths
{
    public static readonly string GenericPowerIconPath = ImageHelper.GetImagePath("powers/power.png");
    public static readonly string GenericBigPowerIconPath = ImageHelper.GetImagePath("powers/big/power.png");
}

internal static class LoudCrowRelicAssetPaths
{
    public static readonly string GenericRelicIconPath = ImageHelper.GetImagePath("relics/relic.png");
    public static readonly string GenericBigRelicIconPath = ImageHelper.GetImagePath("relics/big/relic.png");
    public static readonly string GenericRelicOutlinePath = ImageHelper.GetImagePath("relics/relic_outline.png");

    public static readonly string BulletPouchIconPath = ImageHelper.GetImagePath("relics/bullet_pouch.png");
    public static readonly string BulletPouchBigIconPath = ImageHelper.GetImagePath("relics/big/bullet_pouch.png");
    public static readonly string BulletPouchOutlinePath = ImageHelper.GetImagePath("relics/bullet_pouch_outline.png");

    public static readonly string ShootingTargetIconPath = ImageHelper.GetImagePath("relics/shooting_target.png");
    public static readonly string ShootingTargetBigIconPath = ImageHelper.GetImagePath("relics/big/shooting_target.png");
    public static readonly string ShootingTargetOutlinePath = ImageHelper.GetImagePath("relics/shooting_target_outline.png");

    public static readonly string SheriffBadgeIconPath = ImageHelper.GetImagePath("relics/sheriff_badge.png");
    public static readonly string SheriffBadgeBigIconPath = ImageHelper.GetImagePath("relics/big/sheriff_badge.png");
    public static readonly string SheriffBadgeOutlinePath = ImageHelper.GetImagePath("relics/sheriff_badge_outline.png");

    public static readonly string ShellFragmentIconPath = ImageHelper.GetImagePath("relics/shell_fragment.png");
    public static readonly string ShellFragmentBigIconPath = ImageHelper.GetImagePath("relics/big/shell_fragment.png");
    public static readonly string ShellFragmentOutlinePath = ImageHelper.GetImagePath("relics/shell_fragment_outline.png");

    public static readonly string BlackFeatherIconPath = ImageHelper.GetImagePath("relics/black_feather.png");
    public static readonly string BlackFeatherBigIconPath = ImageHelper.GetImagePath("relics/big/black_feather.png");
    public static readonly string BlackFeatherOutlinePath = ImageHelper.GetImagePath("relics/black_feather_outline.png");

    public static readonly string TornContractIconPath = ImageHelper.GetImagePath("relics/torn_contract.png");
    public static readonly string TornContractBigIconPath = ImageHelper.GetImagePath("relics/big/torn_contract.png");
    public static readonly string TornContractOutlinePath = ImageHelper.GetImagePath("relics/torn_contract_outline.png");

    public static readonly string ExpressMailIconPath = ImageHelper.GetImagePath("relics/express_mail.png");
    public static readonly string ExpressMailBigIconPath = ImageHelper.GetImagePath("relics/big/express_mail.png");
    public static readonly string ExpressMailOutlinePath = ImageHelper.GetImagePath("relics/express_mail_outline.png");

    public static readonly string EchoBulletIconPath = ImageHelper.GetImagePath("relics/echo_bullet.png");
    public static readonly string EchoBulletBigIconPath = ImageHelper.GetImagePath("relics/big/echo_bullet.png");
    public static readonly string EchoBulletOutlinePath = ImageHelper.GetImagePath("relics/echo_bullet_outline.png");

    public static readonly string BandolierIconPath = ImageHelper.GetImagePath("relics/bandolier.png");
    public static readonly string BandolierBigIconPath = ImageHelper.GetImagePath("relics/big/bandolier.png");
    public static readonly string BandolierOutlinePath = ImageHelper.GetImagePath("relics/bandolier_outline.png");
}

internal static class LoudCrowPotionAssetResolver
{
    internal static bool TryGetRawImagePath(PotionModel potion, out string path)
    {
        path = potion switch
        {
            BulletPotion => LoudCrowPotionAssetPaths.BulletPotionPath,
            LavaPotion => LoudCrowPotionAssetPaths.LavaPotionPath,
            MagicBulletPotion => LoudCrowPotionAssetPaths.MagicBulletPotionPath,
            _ => string.Empty,
        };

        return !string.IsNullOrEmpty(path);
    }
}

internal static class LoudCrowRelicAssetResolver
{
    private static string ExistingOrFallback(string preferredPath, string fallbackPath)
    {
        string diskPath = ProjectSettings.GlobalizePath(preferredPath);
        return File.Exists(diskPath) ? preferredPath : fallbackPath;
    }

    internal static bool TryGetPackedIconPath(RelicModel relic, out string path)
    {
        path = relic switch
        {
            BulletPouch => ExistingOrFallback(LoudCrowRelicAssetPaths.BulletPouchIconPath, LoudCrowRelicAssetPaths.GenericRelicIconPath),
            ShootingTarget => ExistingOrFallback(LoudCrowRelicAssetPaths.ShootingTargetIconPath, LoudCrowRelicAssetPaths.GenericRelicIconPath),
            SheriffBadge => ExistingOrFallback(LoudCrowRelicAssetPaths.SheriffBadgeIconPath, LoudCrowRelicAssetPaths.GenericRelicIconPath),
            ShellFragment => ExistingOrFallback(LoudCrowRelicAssetPaths.ShellFragmentIconPath, LoudCrowRelicAssetPaths.GenericRelicIconPath),
            BlackFeather => ExistingOrFallback(LoudCrowRelicAssetPaths.BlackFeatherIconPath, LoudCrowRelicAssetPaths.GenericRelicIconPath),
            TornContract => ExistingOrFallback(LoudCrowRelicAssetPaths.TornContractIconPath, LoudCrowRelicAssetPaths.GenericRelicIconPath),
            ExpressMail => ExistingOrFallback(LoudCrowRelicAssetPaths.ExpressMailIconPath, LoudCrowRelicAssetPaths.GenericRelicIconPath),
            EchoBullet => ExistingOrFallback(LoudCrowRelicAssetPaths.EchoBulletIconPath, LoudCrowRelicAssetPaths.GenericRelicIconPath),
            Bandolier => ExistingOrFallback(LoudCrowRelicAssetPaths.BandolierIconPath, LoudCrowRelicAssetPaths.GenericRelicIconPath),
            _ => string.Empty,
        };

        return !string.IsNullOrEmpty(path);
    }

    internal static bool TryGetBigIconPath(RelicModel relic, out string path)
    {
        path = relic switch
        {
            BulletPouch => ExistingOrFallback(LoudCrowRelicAssetPaths.BulletPouchBigIconPath, LoudCrowRelicAssetPaths.GenericBigRelicIconPath),
            ShootingTarget => ExistingOrFallback(LoudCrowRelicAssetPaths.ShootingTargetBigIconPath, LoudCrowRelicAssetPaths.GenericBigRelicIconPath),
            SheriffBadge => ExistingOrFallback(LoudCrowRelicAssetPaths.SheriffBadgeBigIconPath, LoudCrowRelicAssetPaths.GenericBigRelicIconPath),
            ShellFragment => ExistingOrFallback(LoudCrowRelicAssetPaths.ShellFragmentBigIconPath, LoudCrowRelicAssetPaths.GenericBigRelicIconPath),
            BlackFeather => ExistingOrFallback(LoudCrowRelicAssetPaths.BlackFeatherBigIconPath, LoudCrowRelicAssetPaths.GenericBigRelicIconPath),
            TornContract => ExistingOrFallback(LoudCrowRelicAssetPaths.TornContractBigIconPath, LoudCrowRelicAssetPaths.GenericBigRelicIconPath),
            ExpressMail => ExistingOrFallback(LoudCrowRelicAssetPaths.ExpressMailBigIconPath, LoudCrowRelicAssetPaths.GenericBigRelicIconPath),
            EchoBullet => ExistingOrFallback(LoudCrowRelicAssetPaths.EchoBulletBigIconPath, LoudCrowRelicAssetPaths.GenericBigRelicIconPath),
            Bandolier => ExistingOrFallback(LoudCrowRelicAssetPaths.BandolierBigIconPath, LoudCrowRelicAssetPaths.GenericBigRelicIconPath),
            _ => string.Empty,
        };

        return !string.IsNullOrEmpty(path);
    }

    internal static bool TryGetOutlinePath(RelicModel relic, out string path)
    {
        path = relic switch
        {
            BulletPouch => ExistingOrFallback(LoudCrowRelicAssetPaths.BulletPouchOutlinePath, LoudCrowRelicAssetPaths.GenericRelicOutlinePath),
            ShootingTarget => ExistingOrFallback(LoudCrowRelicAssetPaths.ShootingTargetOutlinePath, LoudCrowRelicAssetPaths.GenericRelicOutlinePath),
            SheriffBadge => ExistingOrFallback(LoudCrowRelicAssetPaths.SheriffBadgeOutlinePath, LoudCrowRelicAssetPaths.GenericRelicOutlinePath),
            ShellFragment => ExistingOrFallback(LoudCrowRelicAssetPaths.ShellFragmentOutlinePath, LoudCrowRelicAssetPaths.GenericRelicOutlinePath),
            BlackFeather => ExistingOrFallback(LoudCrowRelicAssetPaths.BlackFeatherOutlinePath, LoudCrowRelicAssetPaths.GenericRelicOutlinePath),
            TornContract => ExistingOrFallback(LoudCrowRelicAssetPaths.TornContractOutlinePath, LoudCrowRelicAssetPaths.GenericRelicOutlinePath),
            ExpressMail => ExistingOrFallback(LoudCrowRelicAssetPaths.ExpressMailOutlinePath, LoudCrowRelicAssetPaths.GenericRelicOutlinePath),
            EchoBullet => ExistingOrFallback(LoudCrowRelicAssetPaths.EchoBulletOutlinePath, LoudCrowRelicAssetPaths.GenericRelicOutlinePath),
            Bandolier => ExistingOrFallback(LoudCrowRelicAssetPaths.BandolierOutlinePath, LoudCrowRelicAssetPaths.GenericRelicOutlinePath),
            _ => string.Empty,
        };

        return !string.IsNullOrEmpty(path);
    }
}

[HarmonyPatch(typeof(PotionModel), "get_PackedImagePath")]
internal static class LoudCrowPotionPackedImagePathPatch
{
    [HarmonyPrefix]
    static bool Prefix(PotionModel __instance, ref string __result)
    {
        if (!LoudCrowPotionAssetResolver.TryGetRawImagePath(__instance, out string path))
            return true;

        __result = path;
        return false;
    }
}

[HarmonyPatch(typeof(RelicModel), "get_PackedIconPath")]
internal static class LoudCrowRelicPackedIconPathPatch
{
    [HarmonyPrefix]
    static bool Prefix(RelicModel __instance, ref string __result)
    {
        if (!LoudCrowRelicAssetResolver.TryGetPackedIconPath(__instance, out string path))
            return true;

        __result = path;
        return false;
    }
}

[HarmonyPatch(typeof(RelicModel), "get_BigIconPath")]
internal static class LoudCrowRelicBigIconPathPatch
{
    [HarmonyPrefix]
    static bool Prefix(RelicModel __instance, ref string __result)
    {
        if (!LoudCrowRelicAssetResolver.TryGetBigIconPath(__instance, out string path))
            return true;

        __result = path;
        return false;
    }
}

[HarmonyPatch(typeof(RelicModel), "get_PackedIconOutlinePath")]
internal static class LoudCrowRelicPackedIconOutlinePathPatch
{
    [HarmonyPrefix]
    static bool Prefix(RelicModel __instance, ref string __result)
    {
        if (!LoudCrowRelicAssetResolver.TryGetOutlinePath(__instance, out string path))
            return true;

        __result = path;
        return false;
    }
}

[HarmonyPatch(typeof(PowerModel), "get_PackedIconPath")]
internal static class LoudCrowPowerPackedIconPathPatch
{
    [HarmonyPrefix]
    static bool Prefix(PowerModel __instance, ref string __result)
    {
        if (__instance.GetType().Namespace != "LoudCrowMod.Powers")
            return true;

        __result = LoudCrowPowerAssetPaths.GenericPowerIconPath;
        return false;
    }
}

[HarmonyPatch(typeof(PowerModel), "get_IconPath")]
internal static class LoudCrowPowerIconPathPatch
{
    [HarmonyPrefix]
    static bool Prefix(PowerModel __instance, ref string __result)
    {
        if (__instance.GetType().Namespace != "LoudCrowMod.Powers")
            return true;

        __result = LoudCrowPowerAssetPaths.GenericPowerIconPath;
        return false;
    }
}

[HarmonyPatch(typeof(PowerModel), "get_ResolvedBigIconPath")]
internal static class LoudCrowPowerResolvedBigIconPathPatch
{
    [HarmonyPrefix]
    static bool Prefix(PowerModel __instance, ref string __result)
    {
        if (__instance.GetType().Namespace != "LoudCrowMod.Powers")
            return true;

        __result = LoudCrowPowerAssetPaths.GenericBigPowerIconPath;
        return false;
    }
}

[HarmonyPatch(typeof(LocManager), "Initialize")]
internal static class InjectLocalizationInit
{
    [HarmonyPostfix]
    static void Postfix(LocManager __instance)
    {
        LocalizationInjector.Inject(__instance);
    }
}

internal static class LoudCrowCharacterAssetPaths
{
    internal const string CharacterSelectBgPath = "res://scenes/screens/char_select/char_select_bg_ironclad.tscn";
    internal const string CharacterSelectIconPath = "res://images/packed/character_select/char_select_loudcrowmod-loud_crow_character.png";
    internal const string CharacterSelectLockedIconPath = "res://images/packed/character_select/char_select_loudcrowmod-loud_crow_character_locked.png";
    internal const string CharacterSelectTransitionPath = "res://materials/transitions/ironclad_transition_mat.tres";
    internal const string VisualsPath = "res://scenes/creature_visuals/loud_crow.tscn";
    internal const string IconTexturePath = "res://images/ui/top_panel/character_icon_loudcrowmod-loud_crow_character.png";
    internal const string IconPath = "res://scenes/ui/character_icons/ironclad_icon.tscn";
    internal const string EnergyCounterPath = "res://scenes/combat/energy_counters/ironclad_energy_counter.tscn";
    internal const string RestSiteAnimPath = "res://scenes/rest_site/characters/ironclad_rest_site.tscn";
    internal const string MerchantAnimPath = "res://scenes/merchant/characters/ironclad_merchant.tscn";
    internal const string TrailPath = "res://scenes/vfx/card_trail_ironclad.tscn";
    internal const string MapMarkerPath = "res://images/packed/map/icons/map_marker_loud_crow_character.png";
}

[HarmonyPatch(typeof(CharacterModel), "get_CharacterSelectBg")]
internal static class LoudCrowCharacterSelectBgPatch
{
    [HarmonyPrefix]
    static bool Prefix(CharacterModel __instance, ref string __result)
    {
        if (__instance is not LoudCrowCharacter)
            return true;

        __result = LoudCrowCharacterAssetPaths.CharacterSelectBgPath;
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), "get_CharacterSelectIconPath")]
internal static class LoudCrowCharacterSelectIconPathPatch
{
    [HarmonyPrefix]
    static bool Prefix(CharacterModel __instance, ref string __result)
    {
        if (__instance is not LoudCrowCharacter)
            return true;

        __result = LoudCrowCharacterAssetPaths.CharacterSelectIconPath;
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), "get_CharacterSelectLockedIconPath")]
internal static class LoudCrowCharacterSelectLockedIconPathPatch
{
    [HarmonyPrefix]
    static bool Prefix(CharacterModel __instance, ref string __result)
    {
        if (__instance is not LoudCrowCharacter)
            return true;

        __result = LoudCrowCharacterAssetPaths.CharacterSelectLockedIconPath;
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), "get_CharacterSelectTransitionPath")]
internal static class LoudCrowCharacterSelectTransitionPathPatch
{
    [HarmonyPrefix]
    static bool Prefix(CharacterModel __instance, ref string __result)
    {
        if (__instance is not LoudCrowCharacter)
            return true;

        __result = LoudCrowCharacterAssetPaths.CharacterSelectTransitionPath;
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), "get_VisualsPath")]
internal static class LoudCrowCharacterVisualsPathPatch
{
    private static bool Prefix(CharacterModel __instance, ref string __result)
    {
        if (__instance is not LoudCrowCharacter)
            return true;

        __result = LoudCrowCharacterAssetPaths.VisualsPath;
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), "get_IconTexturePath")]
internal static class LoudCrowCharacterIconTexturePathPatch
{
    [HarmonyPrefix]
    static bool Prefix(CharacterModel __instance, ref string __result)
    {
        if (__instance is not LoudCrowCharacter)
            return true;

        __result = LoudCrowCharacterAssetPaths.IconTexturePath;
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), "get_IconPath")]
internal static class LoudCrowCharacterIconPathPatch
{
    private static bool Prefix(CharacterModel __instance, ref string __result)
    {
        if (__instance is not LoudCrowCharacter)
            return true;

        __result = LoudCrowCharacterAssetPaths.IconPath;
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), "get_EnergyCounterPath")]
internal static class LoudCrowCharacterEnergyCounterPathPatch
{
    private static bool Prefix(CharacterModel __instance, ref string __result)
    {
        if (__instance is not LoudCrowCharacter)
            return true;

        __result = LoudCrowCharacterAssetPaths.EnergyCounterPath;
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), "get_RestSiteAnimPath")]
internal static class LoudCrowCharacterRestSiteAnimPathPatch
{
    private static bool Prefix(CharacterModel __instance, ref string __result)
    {
        if (__instance is not LoudCrowCharacter)
            return true;

        __result = LoudCrowCharacterAssetPaths.RestSiteAnimPath;
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), "get_MerchantAnimPath")]
internal static class LoudCrowCharacterMerchantAnimPathPatch
{
    private static bool Prefix(CharacterModel __instance, ref string __result)
    {
        if (__instance is not LoudCrowCharacter)
            return true;

        __result = LoudCrowCharacterAssetPaths.MerchantAnimPath;
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), "get_TrailPath")]
internal static class LoudCrowCharacterTrailPathPatch
{
    private static bool Prefix(CharacterModel __instance, ref string __result)
    {
        if (__instance is not LoudCrowCharacter)
            return true;

        __result = LoudCrowCharacterAssetPaths.TrailPath;
        return false;
    }
}

[HarmonyPatch(typeof(CharacterModel), "get_MapMarkerPath")]
internal static class LoudCrowCharacterMapMarkerPathPatch
{
    [HarmonyPrefix]
    static bool Prefix(CharacterModel __instance, ref string __result)
    {
        if (__instance is not LoudCrowCharacter)
            return true;

        __result = LoudCrowCharacterAssetPaths.MapMarkerPath;
        return false;
    }
}

[HarmonyPatch(typeof(LocManager), "SetLanguage")]
internal static class InjectLocalizationSetLang
{
    [HarmonyPostfix]
    static void Postfix(LocManager __instance)
    {
        if (__instance == null) return;
        LocalizationInjector.Inject(__instance);
    }
}

[HarmonyPatch(typeof(LocManager), "StartOverridingLanguageAsEnglish")]
internal static class InjectLocalizationOverride
{
    [HarmonyPostfix]
    static void Postfix(LocManager __instance)
    {
        if (__instance == null) return;
        LocalizationInjector.Inject(__instance);
        var engTables = (Dictionary<string, LocTable>?)typeof(LocManager)
            .GetField("_engTables", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(__instance);
        if (engTables == null) return;
        if (engTables.TryGetValue("characters", out var charTable))
            LocalizationInjector.InjectTable(charTable, "characters", LocalizationInjector.CharEntries);
        if (engTables.TryGetValue("cards", out var cardTable))
            LocalizationInjector.InjectTable(cardTable, "cards", LocalizationInjector.CardEntries);
        if (engTables.TryGetValue("powers", out var powerTable))
            LocalizationInjector.InjectTable(powerTable, "powers", LocalizationInjector.PowerEntries);
        if (engTables.TryGetValue("relics", out var relicTable))
            LocalizationInjector.InjectTable(relicTable, "relics", LocalizationInjector.RelicEntries);
        if (engTables.TryGetValue("ancients", out var ancientTable))
            LocalizationInjector.InjectTable(ancientTable, "ancients", LocalizationInjector.AncientEntries);
        if (engTables.TryGetValue("potions", out var potionTable))
            LocalizationInjector.InjectTable(potionTable, "potions", LocalizationInjector.PotionEntries);
    }
}

[HarmonyPatch(typeof(LocManager), "StopOverridingLanguageAsEnglish")]
internal static class InjectLocalizationStopOverride
{
    [HarmonyPostfix]
    static void Postfix(LocManager __instance)
    {
        if (__instance == null) return;
        LocalizationInjector.Inject(__instance);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForPile), new[] { typeof(PileType), typeof(Creature) })]
internal static class FixLoudCrowCardDescriptionPreview
{
    [HarmonyPostfix]
    static void Postfix(CardModel __instance, PileType pileType, ref string __result)
    {
        __result = LoudCrowDescriptionPreviewRenderer.Render(__instance, __result, pileType);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForUpgradePreview))]
internal static class FixLoudCrowCardUpgradePreviewDescription
{
    [HarmonyPostfix]
    static void Postfix(CardModel __instance, ref string __result)
    {
        __result = LoudCrowDescriptionPreviewRenderer.Render(__instance, __result, PileType.None);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.EndOfTurnCleanup))]
internal static class ResetLoudCrowBulletTurnCounter
{
    [HarmonyPostfix]
    static void Postfix(CardModel __instance)
    {
        var owner = __instance.Owner?.Creature;
        if (owner == null)
            return;

        TaskHelper.RunSafely(LoudCrowTurnEffects.ResolveEnemyTurnStartEffects(owner));
        BulletSystem.ResetAttackBulletsConsumedThisTurn(owner);
        LoudCrowTurnEffects.MarkTurnEnded(owner);
    }
}

[HarmonyPatch(typeof(CardModel), "get_Pool")]
internal static class LoudCrowCardPoolResolutionPatch
{
    [HarmonyPrefix]
    static bool Prefix(CardModel __instance, ref CardPoolModel __result)
    {
        var type = __instance.GetType();
        if (!typeof(LoudCrowCardModel).IsAssignableFrom(type))
            return true;

        if (!LoudCrowCardPoolResolver.TryResolve(type, out __result))
            return true;

        LoudCrowCardPoolResolver.Bind(__instance, __result);
        return false;
    }
}

internal static class LoudCrowCardPoolResolver
{
    private static readonly FieldInfo? PoolField =
        typeof(CardModel).GetField("_pool", BindingFlags.NonPublic | BindingFlags.Instance);

    internal static bool TryResolve(Type cardType, out CardPoolModel pool)
    {
        foreach (var attr in cardType.CustomAttributes)
        {
            if (attr.AttributeType.Name != "PoolAttribute" || attr.ConstructorArguments.Count == 0)
                continue;

            var poolType = attr.ConstructorArguments[0].Value as Type;
            if (poolType == null)
                continue;

            if (poolType == typeof(MegaCrit.Sts2.Core.Models.CardPools.StatusCardPool))
            {
                pool = ModelDb.CardPool<MegaCrit.Sts2.Core.Models.CardPools.StatusCardPool>();
                return true;
            }

            if (poolType == typeof(MegaCrit.Sts2.Core.Models.CardPools.TokenCardPool))
            {
                pool = ModelDb.CardPool<MegaCrit.Sts2.Core.Models.CardPools.TokenCardPool>();
                return true;
            }
        }

        pool = ModelDb.CardPool<LoudCrowCardPool>();
        return true;
    }

    internal static void Bind(CardModel card, CardPoolModel pool)
    {
        try
        {
            PoolField?.SetValue(card, pool);
        }
        catch
        {
        }
    }
}

[HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Draw), new[] { typeof(PlayerChoiceContext), typeof(decimal), typeof(MegaCrit.Sts2.Core.Entities.Players.Player), typeof(bool) })]
internal static class ResolveLoudCrowTurnStartEffects
{
    [HarmonyPostfix]
    static void Postfix(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Players.Player player,
        bool fromHandDraw,
        Task<IEnumerable<CardModel>> __result)
    {
        TaskHelper.RunSafely(ResolveAsync(choiceContext, player, fromHandDraw, __result));
    }

    private static async Task ResolveAsync(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Players.Player player,
        bool fromHandDraw,
        Task<IEnumerable<CardModel>> drawTask)
    {
        IEnumerable<CardModel> drawnCards = await drawTask;
        await LoudCrowTurnEffects.AfterCardsDrawn(choiceContext, player, drawnCards, fromHandDraw);
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
internal static class LoudCrowAfterCardPlayedEffects
{
    [HarmonyPostfix]
    static void Postfix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = ResolveAsync(choiceContext, cardPlay, __result);
    }

    private static async Task ResolveAsync(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        Task hookTask)
    {
        await hookTask;
        await LoudCrowTurnEffects.AfterCardPlayed(choiceContext, cardPlay);
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardChangedPiles))]
internal static class LoudCrowAfterCardChangedPilesEffects
{
    [HarmonyPostfix]
    static void Postfix(CardModel card, PileType oldPile, ref Task __result)
    {
        __result = ResolveAsync(card, oldPile, __result);
    }

    private static async Task ResolveAsync(CardModel card, PileType oldPile, Task hookTask)
    {
        await hookTask;
        var newPile = card.Pile?.Type;
        if (oldPile == PileType.Hand && newPile != PileType.Hand)
            LoudCrowTurnEffects.AfterCardLeftHand(card, preserveExhaust: newPile == PileType.Play);

        if (oldPile != PileType.Hand && newPile == PileType.Hand)
            await LoudCrowTurnEffects.AfterCardEnteredHand(card);

        await LoudCrowTurnEffects.AfterCardChangedPiles(card, oldPile);

        if (card is SpecRare02 &&
            (oldPile == PileType.Discard || newPile == PileType.Discard))
        {
            await LoudCrowTurnEffects.SyncSpecRare02Power(card);
        }
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardExhausted))]
internal static class LoudCrowAfterCardExhaustedEffects
{
    [HarmonyPostfix]
    static void Postfix(PlayerChoiceContext choiceContext, CardModel card, Task __result)
    {
        if (card is not Heat)
            return;

        TaskHelper.RunSafely(ResolveAsync(choiceContext, card, __result));
    }

    private static async Task ResolveAsync(PlayerChoiceContext choiceContext, CardModel card, Task hookTask)
    {
        await hookTask;
        await LoudCrowTurnEffects.OnHeatExhausted(choiceContext, card);
    }
}

[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.GainBlock), new[] { typeof(Creature), typeof(decimal), typeof(ValueProp), typeof(CardPlay), typeof(bool) })]
internal static class LoudCrowTrackBlockGain
{
    [HarmonyPrefix]
    static void Prefix(Creature creature, ref decimal __state)
    {
        __state = LoudCrowCardLogic.GetCreatureBlock(creature) ?? 0M;
    }

    [HarmonyPostfix]
    static void Postfix(Creature creature, Task<decimal> __result, decimal __state)
    {
        TaskHelper.RunSafely(ResolveAsync(creature, __result, __state));
    }

    private static async Task ResolveAsync(Creature creature, Task<decimal> gainTask, decimal blockBefore)
    {
        await gainTask;
        decimal blockAfter = LoudCrowCardLogic.GetCreatureBlock(creature) ?? blockBefore;
        decimal gained = System.Math.Max(0M, blockAfter - blockBefore);
        LoudCrowTurnEffects.RecordBlockGained(creature, gained);
    }
}

[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.GainBlock), new[] { typeof(Creature), typeof(BlockVar), typeof(CardPlay), typeof(bool) })]
internal static class LoudCrowTrackBlockGainFromVar
{
    [HarmonyPrefix]
    static void Prefix(Creature creature, ref decimal __state)
    {
        __state = LoudCrowCardLogic.GetCreatureBlock(creature) ?? 0M;
    }

    [HarmonyPostfix]
    static void Postfix(Creature creature, Task<decimal> __result, decimal __state)
    {
        TaskHelper.RunSafely(ResolveAsync(creature, __result, __state));
    }

    private static async Task ResolveAsync(Creature creature, Task<decimal> gainTask, decimal blockBefore)
    {
        await gainTask;
        decimal blockAfter = LoudCrowCardLogic.GetCreatureBlock(creature) ?? blockBefore;
        decimal gained = System.Math.Max(0M, blockAfter - blockBefore);
        LoudCrowTurnEffects.RecordBlockGained(creature, gained);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
internal static class ReadyToFireSpendResourcesPatch
{
    [HarmonyPrefix]
    static void Prefix(CardModel __instance)
    {
        try
        {
            var owner = __instance.Owner?.Creature;
            if (owner == null)
                return;

            if (!LoudCrowTurnEffects.IsShotCard(__instance))
                return;

            if (owner.GetPower(ModelDb.GetId(typeof(ReadyToFirePower))) is not ReadyToFirePower ready)
                return;

            __instance.SetToFreeThisTurn();
            ready.RemoveInternal();
            if (__instance.Owner != null)
                LoudCrowCardLogic.RefreshHandVisuals(__instance.Owner);
        }
        catch
        {
        }
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
internal static class TableTurningSpendResourcesPatch
{
    [HarmonyPrefix]
    static void Prefix(CardModel __instance)
    {
        try
        {
            if (!LoudCrowCardLogic.IsTableTurning(__instance))
                return;

            LoudCrowCardCostHelpers.SetCardEnergyCost(__instance, LoudCrowCardLogic.GetTableTurningCost(__instance!));
        }
        catch
        {
        }
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
internal static class ReadyToFireCostDisplayPatch
{
    [HarmonyPostfix]
    static void Postfix(NCard __instance)
    {
        try
        {
            var model = __instance.Model;
            if (__instance.DisplayingPile != PileType.Hand)
                return;
            if (!LoudCrowCardLogic.IsShotCard(model))
                return;

            if (model == null)
                return;

            var owner = model.Owner?.Creature;
            if (owner == null)
                return;
            if (owner.GetPower(ModelDb.GetId(typeof(ReadyToFirePower))) == null)
                return;

            var energyLabel = typeof(NCard)
                .GetField("_energyLabel", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(__instance) as MegaLabel;
            var energyIcon = typeof(NCard)
                .GetField("_energyIcon", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(__instance) as TextureRect;

            energyLabel?.SetTextAutoSize("0");
            if (energyIcon != null)
                energyIcon.Visible = true;
        }
        catch
        {
        }
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
internal static class TableTurningCostDisplayPatch
{
    [HarmonyPostfix]
    static void Postfix(NCard __instance)
    {
        try
        {
            var model = __instance.Model;
            if (__instance.DisplayingPile != PileType.Hand)
                return;
            if (!LoudCrowCardLogic.IsTableTurning(model))
                return;

            int cost = LoudCrowCardLogic.GetTableTurningCost(model!);
            LoudCrowCardCostHelpers.SetCardEnergyCost(model!, cost);

            var energyLabel = typeof(NCard)
                .GetField("_energyLabel", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(__instance) as MegaLabel;
            var energyIcon = typeof(NCard)
                .GetField("_energyIcon", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(__instance) as TextureRect;

            energyLabel?.SetTextAutoSize(cost.ToString());
            if (energyIcon != null)
                energyIcon.Visible = true;
        }
        catch
        {
        }
    }
}

internal static class LoudCrowCardCostHelpers
{
    internal static void SetCardEnergyCost(CardModel card, int cost)
    {
        try
        {
            var mockSetEnergyCost = typeof(CardModel).GetMethod(
                "MockSetEnergyCost",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (mockSetEnergyCost == null)
                return;

            mockSetEnergyCost.Invoke(card, [new CardEnergyCost(card, cost, false)]);
        }
        catch
        {
        }
    }
}

[HarmonyPatch(typeof(NCardLibrary), "_Ready")]
internal static class FixLoudCrowCardLibraryPredicate
{
    [HarmonyPostfix]
    static void Postfix(NCardLibrary __instance)
    {
        try
        {
            var poolFilters = (Dictionary<NCardPoolFilter, System.Func<CardModel, bool>>?)typeof(NCardLibrary)
                .GetField("_poolFilters", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(__instance);
            if (poolFilters == null)
                return;

            var builtinFilters = new HashSet<NCardPoolFilter>(
                GetBuiltinPoolFilters(__instance).Where(filter => filter != null)!);

            var loudCrowFilters = EnumerateNodes(__instance)
                .OfType<NCardPoolFilter>()
                .Where(filter =>
                    !builtinFilters.Contains(filter) &&
                    IsLoudCrowFilter(filter))
                .ToList();

            RemoveDuplicateLoudCrowFilters(loudCrowFilters, poolFilters);

            var loudCrowFilter = loudCrowFilters.FirstOrDefault();

            if (loudCrowFilter == null)
                return;

            var ironcladFilter = typeof(NCardLibrary)
                .GetField("_ironcladFilter", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(__instance) as NCardPoolFilter;
            if (ironcladFilter != null)
                TryCopyFilterSelectionGroup(ironcladFilter, loudCrowFilter);

            poolFilters[loudCrowFilter] = card =>
                card.ShouldShowInCardLibrary &&
                card.Pool is LoudCrowCardPool;

            WireExclusiveLoudCrowFilterBehavior(__instance, loudCrowFilter, poolFilters.Keys);
        }
        catch
        {
        }
    }

    private static void RemoveDuplicateLoudCrowFilters(
        List<NCardPoolFilter> loudCrowFilters,
        Dictionary<NCardPoolFilter, System.Func<CardModel, bool>> poolFilters)
    {
        if (loudCrowFilters.Count <= 1)
            return;

        var keep = loudCrowFilters[0];
        foreach (var duplicate in loudCrowFilters.Skip(1).ToList())
        {
            try
            {
                poolFilters.Remove(duplicate);
                duplicate.GetParent()?.RemoveChild(duplicate);
                duplicate.QueueFree();
                loudCrowFilters.Remove(duplicate);
            }
            catch
            {
            }
        }

        if (!loudCrowFilters.Contains(keep))
            loudCrowFilters.Insert(0, keep);
    }

    private static IEnumerable<NCardPoolFilter?> GetBuiltinPoolFilters(NCardLibrary library)
    {
        string[] fieldNames =
        [
            "_ironcladFilter",
            "_silentFilter",
            "_defectFilter",
            "_regentFilter",
            "_necrobinderFilter",
            "_colorlessFilter",
            "_ancientsFilter",
            "_miscPoolFilter",
        ];

        foreach (string fieldName in fieldNames)
        {
            yield return (NCardPoolFilter?)typeof(NCardLibrary)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(library);
        }
    }

    private static IEnumerable<Node> EnumerateNodes(Node root)
    {
        yield return root;

        foreach (Node child in root.GetChildren())
        {
            foreach (Node descendant in EnumerateNodes(child))
                yield return descendant;
        }
    }

    private static bool IsLoudCrowFilter(NCardPoolFilter filter)
    {
        try
        {
            var loc = filter.Loc;
            if (loc == null)
                return false;

            if (loc.LocTable == "characters" &&
                (loc.LocEntryKey == "LOUDCROWMOD-LOUD_CROW_CHARACTER.title" ||
                 loc.LocEntryKey == "LOUD_CROW_CHARACTER.title"))
                return true;

            string text = loc.GetFormattedText();
            return text == "Loud Crow";
        }
        catch
        {
            return false;
        }
    }

    private static void WireExclusiveLoudCrowFilterBehavior(
        NCardLibrary library,
        NCardPoolFilter loudCrowFilter,
        IEnumerable<NCardPoolFilter> allFilters)
    {
        loudCrowFilter.Toggled += _ =>
        {
            SetOnlyThisFilterPressed(loudCrowFilter, allFilters);
            AccessTools.Method(typeof(NCardLibrary), "UpdateCardPoolFilter")
                ?.Invoke(library, [loudCrowFilter]);
        };
    }

    private static void SetOnlyThisFilterPressed(
        NCardPoolFilter selectedFilter,
        IEnumerable<NCardPoolFilter> allFilters)
    {
        foreach (var filter in allFilters)
        {
            if (filter == null || ReferenceEquals(filter, selectedFilter))
                continue;

            TrySetFilterPressed(filter, false);
        }

        TrySetFilterPressed(selectedFilter, true);
    }

    private static void TrySetFilterPressed(NCardPoolFilter filter, bool value)
    {
        try
        {
            var isSelectedProperty = filter.GetType().GetProperty("IsSelected", BindingFlags.Public | BindingFlags.Instance);
            if (isSelectedProperty?.CanWrite == true && isSelectedProperty.PropertyType == typeof(bool))
            {
                isSelectedProperty.SetValue(filter, value);
                return;
            }

            var property = filter.GetType().GetProperty("ButtonPressed", BindingFlags.Public | BindingFlags.Instance);
            if (property?.CanWrite == true && property.PropertyType == typeof(bool))
            {
                property.SetValue(filter, value);
                return;
            }

            var method = filter.GetType().GetMethod("SetPressedNoSignal", BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(filter, [value]);
                return;
            }
        }
        catch
        {
        }
    }

    private static void TryConnectLibraryPoolFilter(NCardLibrary library, NCardPoolFilter filter)
    {
        try
        {
            var updateCardPoolFilter = AccessTools.Method(typeof(NCardLibrary), "UpdateCardPoolFilter");
            if (updateCardPoolFilter == null)
                return;

            var callable = Callable.From<NCardPoolFilter>(poolFilter =>
            {
                updateCardPoolFilter.Invoke(library, [poolFilter]);
            });

            filter.Connect(NCardPoolFilter.SignalName.Toggled, callable);
        }
        catch
        {
        }
    }

    private static void TryCopyFilterSelectionGroup(NCardPoolFilter source, NCardPoolFilter target)
    {
        try
        {
            var sourceType = source.GetType();
            var targetType = target.GetType();

            var groupProperty = sourceType.GetProperty("ButtonGroup", BindingFlags.Public | BindingFlags.Instance);
            var targetGroupProperty = targetType.GetProperty("ButtonGroup", BindingFlags.Public | BindingFlags.Instance);
            if (groupProperty?.CanRead == true && targetGroupProperty?.CanWrite == true)
            {
                var group = groupProperty.GetValue(source);
                if (group != null)
                    targetGroupProperty.SetValue(target, group);
            }

            var toggleModeProperty = targetType.GetProperty("ToggleMode", BindingFlags.Public | BindingFlags.Instance);
            if (toggleModeProperty?.CanWrite == true && toggleModeProperty.PropertyType == typeof(bool))
                toggleModeProperty.SetValue(target, true);
        }
        catch
        {
        }
    }
}

[HarmonyPatch(typeof(TouchOfOrobas), "get_RefinementUpgrades")]
internal static class LoudCrowTouchOfOrobasRefinementPatch
{
    [HarmonyPostfix]
    static void Postfix(ref Dictionary<ModelId, RelicModel> __result)
    {
        __result ??= new Dictionary<ModelId, RelicModel>();

        var starter = ModelDb.Relic<BulletPouch>();
        var upgraded = ModelDb.Relic<Bandolier>();

        __result[starter.Id] = upgraded;
    }
}

[HarmonyPatch(typeof(ArchaicTooth), "get_TranscendenceUpgrades")]
internal static class LoudCrowArchaicToothTranscendencePatch
{
    [HarmonyPostfix]
    static void Postfix(ref Dictionary<ModelId, CardModel> __result)
    {
        __result ??= new Dictionary<ModelId, CardModel>();
        __result[ModelDb.Card<LoudCrowBurst>().Id] = ModelDb.Card<FullBurst>();
    }
}

[HarmonyPatch(typeof(Storybook), "get_ExtraHoverTips")]
internal static class LoudCrowStorybookHoverPatch
{
    [HarmonyPostfix]
    static void Postfix(Storybook __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (__instance?.Owner?.Character is not LoudCrowCharacter)
            return;

        __result = HoverTipFactory.FromCardWithCardHoverTips<MagicBulletMarksman>();
    }
}

[HarmonyPatch(typeof(Storybook), nameof(Storybook.AfterObtained))]
internal static class LoudCrowStorybookAfterObtainedPatch
{
    [HarmonyPrefix]
    static bool Prefix(Storybook __instance, ref Task __result)
    {
        if (__instance?.Owner?.Character is not LoudCrowCharacter)
            return true;

        __result = GiveLoudCrowAncient(__instance);
        return false;
    }

    private static async Task GiveLoudCrowAncient(Storybook storybook)
    {
        var owner = storybook.Owner;
        var card = owner.RunState.CreateCard<MagicBulletMarksman>(owner);
        CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck);
        CardCmd.PreviewCardPileAdd([result], 2f);
    }
}






