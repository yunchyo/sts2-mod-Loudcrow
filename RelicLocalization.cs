using System.Collections.Generic;

namespace LoudCrowMod;

internal static class RelicLocalization
{
    internal static readonly Dictionary<string, string> Entries = CreateEntries();

    private static Dictionary<string, string> CreateEntries()
    {
        var entries = new Dictionary<string, string>();

        AddRelic(entries, "BULLET_POUCH", "Bullet Pouch",
            $"At the start of each combat, gain 2 random {LoudCrowTextStyles.Bullets}.",
            "Always be prepared.");
        AddRelic(entries, "SHOOTING_TARGET", "\uC0AC\uACA9\uC6A9 \uD45C\uC801",
            "\uBAA8\uB4E0 \uC0AC\uACA9 \uCE74\uB4DC\uC758 \uD53C\uD574\uB7C9\uC774 4 \uC99D\uAC00\uD569\uB2C8\uB2E4.",
            "\uC815\uD655\uD788 \uC544\uD508 \uACF3\uC744 \uB178\uB9B0\uB2E4.");
        AddRelic(entries, "SHERIFF_BADGE", "\uBCF4\uC548\uAD00 \uBC43\uC9C0",
            $"Whenever you create {LoudCrowTextStyles.Heat}, gain 3 {LoudCrowTextStyles.Block} for each one created.",
            "\uC5F4\uAE30\uAE4C\uC9C0\uB3C4 \uD1B5\uC81C\uC758 \uC77C\uBD80\uB2E4.");
        AddRelic(entries, "SHELL_FRAGMENT", "\uD0C4\uD53C\uC870\uAC01",
            $"Whenever you consume a {LoudCrowTextStyles.Bullet}, deal 2 damage to a random enemy.",
            "\uD280\uC5B4 \uC624\uB978 \uD30C\uD3B8\uB3C4 \uB204\uAD70\uAC00\uC5D0\uAC90 \uCD1D\uC54C\uC774\uB2E4.");
        AddRelic(entries, "BLACK_FEATHER", "\uAC80\uC740 \uAE43\uD138",
            "\uB514\uBC84\uD504\uB97C \uC5BB\uC744 \uB54C\uB9C8\uB2E4 \uC774\uBC88 \uD134\uC5D0 \uC77C\uC2DC\uC801\uC778 \uD798\uC744 1 \uC5BB\uC2B5\uB2C8\uB2E4.",
            "\uBD88\uAE38\uD55C \uC9D5\uC870\uC77C\uC218\uB85D \uC0AC\uACA9\uC740 \uB354 \uB0A0\uCE74\uB85C\uC6CC\uC9C4\uB2E4.");
        AddRelic(entries, "TORN_CONTRACT", "\uCC22\uACA8\uC9C4 \uACC4\uC57D\uC11C",
            $"{LoudCrowTextStyles.MagicBullet} \uBC1C\uB3D9 \uC2DC {LoudCrowTextStyles.Block} 6\uC744 \uC5BB\uC2B5\uB2C8\uB2E4.",
            "\uB0A8\uC740 \uAC83\uC740 \uADDC\uCE59\uC774 \uC544\uB2C8\uB77C \uC9D1\uCC29\uC774\uB2E4.");
        AddRelic(entries, "EXPRESS_MAIL", "\uD2B9\uAE09 \uC6B0\uD3B8",
            $"\uB9E4 \uC804\uD22C \uC2DC\uC791 \uC2DC, \uBB34\uC791\uC704 {LoudCrowTextStyles.Exhaust} \uCE74\uB4DC 1\uC7A5\uC744 \uC190\uC73C\uB85C \uAC00\uC838\uC635\uB2C8\uB2E4.",
            "\uC624\uB798 \uBB36\uC5B4\uB454 \uC57D\uC18D\uB3C4 \uC804\uC7A5\uC5D0\uC120 \uBC30\uB2EC\uB41C\uB2E4.");
        AddRelic(entries, "ECHO_BULLET", "Echo Bullet",
            $"{LoudCrowTextStyles.MagicBullet}\uC774 \uC804\uD22C \uB2F9 \uCC98\uC74C \uBC1C\uB3D9\uD560 \uB54C, \uADF8 \uCE74\uB4DC\uC758 \uBE44\uC6A9\uC774 0\uC778 \uBCF5\uC0AC\uBCF8 1\uC7A5\uC744 \uC190\uC73C\uB85C \uAC00\uC838\uC635\uB2C8\uB2E4. \uADF8 \uBCF5\uC0AC\uBCF8\uC740 {LoudCrowTextStyles.Exhaust}\uB97C \uC5BB\uC2B5\uB2C8\uB2E4.",
            "\uBA00\uC5B4\uC9C4 \uD55C \uBC1C\uB3C4 \uB2E4\uC2DC \uC6B8\uB9B0\uB2E4.");
        AddRelic(entries, "BANDOLIER", "Bandolier",
            $"\uB9E4 \uC804\uD22C \uC2DC\uC791 \uC2DC, {LoudCrowTextStyles.Bullets} 6\uAC1C\uB97C \uC5BB\uC2B5\uB2C8\uB2E4.",
            "");
        return entries;
    }

    private static void AddRelic(Dictionary<string, string> entries, string baseKey, string title, string description, string flavorText)
    {
        entries[$"LOUDCROWMOD-{baseKey}.title"] = title;
        entries[$"LOUDCROWMOD-{baseKey}.description"] = description;
        entries[$"LOUDCROWMOD-{baseKey}.flavorText"] = flavorText;

        entries[$"{baseKey}.title"] = title;
        entries[$"{baseKey}.description"] = description;
        entries[$"{baseKey}.flavorText"] = flavorText;
    }
}
