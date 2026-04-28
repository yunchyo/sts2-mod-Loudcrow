using MegaCrit.Sts2.Core.Localization;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Godot;

namespace LoudCrowMod;

internal static class LocalizationInjector
{
    internal static Dictionary<string, string> CardEntries => CardLocalization.Entries;
    internal static Dictionary<string, string> CharEntries => CharacterLocalization.Entries;
    internal static Dictionary<string, string> PowerEntries => PowerLocalization.Entries;
    internal static Dictionary<string, string> RelicEntries => RelicLocalization.Entries;
    internal static Dictionary<string, string> AncientEntries => AncientLocalization.Entries;
    internal static Dictionary<string, string> PotionEntries => PotionLocalization.Entries;
    private static readonly Dictionary<string, Dictionary<string, string>> JsonCache = new(StringComparer.OrdinalIgnoreCase);

    public static void Inject(LocManager instance)
    {
        if (instance == null)
            return;

        try
        {
            var cards = instance.GetTable("cards");
            var characters = instance.GetTable("characters");
            var powers = instance.GetTable("powers");
            var relics = instance.GetTable("relics");
            var ancients = instance.GetTable("ancients");
            var potions = instance.GetTable("potions");

            ApplyTable(instance, cards, "cards", CardEntries);
            ApplyTable(instance, characters, "characters", CharEntries);
            ApplyTable(instance, powers, "powers", PowerEntries);
            ApplyTable(instance, relics, "relics", RelicEntries);
            ApplyTable(instance, ancients, "ancients", AncientEntries);
            ApplyTable(instance, potions, "potions", PotionEntries);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[LoudCrowMod] Localization injection failed: {ex}");
        }
    }

    internal static void InjectTable(object table, string tableName, IReadOnlyDictionary<string, string> fallbackEntries)
    {
        if (table == null)
            return;

        var merged = BuildMergedEntries("eng", tableName, fallbackEntries);
        InjectEntriesSafely(table, tableName, merged);
    }

    private static void ApplyTable(
        LocManager instance,
        object table,
        string tableName,
        IReadOnlyDictionary<string, string> fallbackEntries)
    {
        var merged = BuildMergedEntries(instance, tableName, fallbackEntries);
        InjectEntriesSafely(table, tableName, merged);
    }

    private static IReadOnlyDictionary<string, string> BuildMergedEntries(
        LocManager instance,
        string tableName,
        IReadOnlyDictionary<string, string> fallbackEntries)
    {
        string currentLanguage = instance.Language ?? "eng";
        return BuildMergedEntries(currentLanguage, tableName, fallbackEntries);
    }

    private static IReadOnlyDictionary<string, string> BuildMergedEntries(
        string currentLanguage,
        string tableName,
        IReadOnlyDictionary<string, string> fallbackEntries)
    {
        Dictionary<string, string> merged = new(StringComparer.Ordinal);
        MergeMissing(merged, LoadJsonTable(currentLanguage, tableName));

        if (!string.Equals(currentLanguage, "eng", StringComparison.OrdinalIgnoreCase))
            MergeMissing(merged, LoadJsonTable("eng", tableName));

        MergeMissing(merged, fallbackEntries);
        return merged;
    }

    private static IReadOnlyDictionary<string, string> LoadJsonTable(string language, string tableName)
    {
        string cacheKey = $"{language}/{tableName}";
        if (JsonCache.TryGetValue(cacheKey, out var cached))
            return cached;

        string resourcePath = $"res://loudcrow_localization/{language}/{tableName}.json";
        Dictionary<string, string> result = new(StringComparer.Ordinal);

        try
        {
            if (Godot.FileAccess.FileExists(resourcePath))
            {
                using var file = Godot.FileAccess.Open(resourcePath, Godot.FileAccess.ModeFlags.Read);
                string json = file?.GetAsText() ?? string.Empty;
                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (parsed != null)
                    result = new Dictionary<string, string>(parsed, StringComparer.Ordinal);
            }
            else
            {
                string absolutePath = ProjectSettings.GlobalizePath(resourcePath);
                if (File.Exists(absolutePath))
                {
                    string json = File.ReadAllText(absolutePath);
                    var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (parsed != null)
                        result = new Dictionary<string, string>(parsed, StringComparer.Ordinal);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[LoudCrowMod] Failed to load localization json '{resourcePath}': {ex}");
        }

        JsonCache[cacheKey] = result;
        return result;
    }

    private static void MergeMissing(
        IDictionary<string, string> target,
        IReadOnlyDictionary<string, string> source)
    {
        foreach (var pair in source)
        {
            if (!target.ContainsKey(pair.Key))
                target[pair.Key] = pair.Value;
        }
    }

    private static void InjectEntriesSafely(object table, string tableName, IReadOnlyDictionary<string, string> entries)
    {
        foreach (var pair in entries)
        {
            try
            {
                if (ShouldReplaceExistingKey(pair.Key) || !TableContainsKey(table, pair.Key))
                    ForceSetEntry(table, pair.Key, pair.Value);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[LoudCrowMod] Failed to inject '{tableName}:{pair.Key}': {ex}");
            }
        }
    }

    private static bool ShouldReplaceExistingKey(string key)
    {
        return key.StartsWith("LOUDCROWMOD-", StringComparison.Ordinal);
    }

    private static bool TableContainsKey(object table, string key)
    {
        var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        foreach (var field in table.GetType().GetFields(flags))
        {
            if (field.GetValue(table) is not IDictionary dictionary)
                continue;

            if (dictionary.Contains(key))
                return true;
        }

        return false;
    }

    private static void ForceSetEntry(object table, string key, string value)
    {
        var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        foreach (var field in table.GetType().GetFields(flags))
        {
            if (field.GetValue(table) is not IDictionary dictionary)
                continue;

            dictionary[key] = value;
        }
    }
}
