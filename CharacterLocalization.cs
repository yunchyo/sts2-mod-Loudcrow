using System.Collections.Generic;

namespace LoudCrowMod;

internal static class CharacterLocalization
{
    internal static readonly Dictionary<string, string> Entries = CreateEntries();

    private static Dictionary<string, string> CreateEntries()
    {
        var entries = new Dictionary<string, string>();

        AddCharacterKey(entries, "title", "Loud Crow");
        AddCharacterKey(entries, "pronounSubject", "he");
        AddCharacterKey(entries, "pronounObject", "him");
        AddCharacterKey(entries, "pronounPossessive", "his");
        AddCharacterKey(entries, "possessiveAdjective", "his");
        AddCharacterKey(entries, "titleObject", "Loud Crow");
        AddCharacterKey(entries, "description", "The Scarecrow Sheriff.");
        AddCharacterKey(entries, "eventDeathPrevention", "Loud Crow refuses to fall!");
        AddCharacterKey(entries, "cardsModifierTitle", "Loud Crow Cards");
        AddCharacterKey(entries, "cardsModifierDescription", "Loud Crow exclusive cards.");
        AddCharacterKey(entries, "unlockText", "Unlocks after beating the game as {Prerequisite}.");

        return entries;
    }

    private static void AddCharacterKey(Dictionary<string, string> entries, string suffix, string value)
    {
        entries[$"LOUDCROWMOD-LOUD_CROW_CHARACTER.{suffix}"] = value;
        entries[$"LOUD_CROW_CHARACTER.{suffix}"] = value;
    }
}
