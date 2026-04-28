using System.Collections.Generic;

namespace LoudCrowMod;

internal static class PotionLocalization
{
    internal static readonly Dictionary<string, string> Entries = new()
    {
        ["LOUDCROWMOD-BULLET_POTION.title"] = "탄환 포션",
        ["LOUDCROWMOD-BULLET_POTION.description"] = $"Gain 3 random {LoudCrowTextStyles.Bullets}.",
        ["LOUDCROWMOD-BULLET_POTION.selection_prompt"] = "탄환 3개를 얻습니다.",
        ["LOUDCROWMOD-LAVA_POTION.title"] = "용암 포션",
        ["LOUDCROWMOD-LAVA_POTION.description"] = "Gain 8 Vigor.",
        ["LOUDCROWMOD-LAVA_POTION.selection_prompt"] = "활력 8을 얻습니다.",
        ["LOUDCROWMOD-MAGIC_BULLET_POTION.title"] = "마탄 포션",
        ["LOUDCROWMOD-MAGIC_BULLET_POTION.description"] = "Gain 1 Magic Bullet.",
        ["LOUDCROWMOD-MAGIC_BULLET_POTION.selection_prompt"] = "마탄 1을 얻습니다.",
    };
}
