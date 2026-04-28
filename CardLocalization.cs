using System.Collections.Generic;

namespace LoudCrowMod;

internal static class CardLocalization
{
    internal static readonly Dictionary<string, string> Entries = new()
    {
        ["LOUDCROWMOD-STRIKE.title"] = "strike",
        ["LOUDCROWMOD-STRIKE.description"] = "Deal {Damage:diff} damage.",
        ["LOUDCROWMOD-DEFEND.title"] = "Defend",
        ["LOUDCROWMOD-DEFEND.description"] = $"Gain {{Block:diff}} {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-LOUD_CROW_BURST.title"] = "Burst",
        ["LOUDCROWMOD-LOUD_CROW_BURST.description"] = $"Discard a card. Deal {{Damage:diff}} damage to ALL enemies. {LoudCrowTextStyles.MagicBullet}: Deal {{EmptyHandDamage:diff}} instead if your hand is empty.",
        ["LOUDCROWMOD-BURST.title"] = "Burst",
        ["LOUDCROWMOD-BURST.description"] = $"Discard a card. Deal {{Damage:diff}} damage to ALL enemies. {LoudCrowTextStyles.MagicBullet}: Deal {{EmptyHandDamage:diff}} instead if your hand is empty.",
        ["LOUDCROWMOD-MIDNIGHT.title"] = "Midnight",
        ["LOUDCROWMOD-MIDNIGHT.description"] = $"Deal {{Damage:diff}} damage. {LoudCrowTextStyles.MagicBullet}: Deal {{MagicBulletDamage:diff}} instead.",
        ["LOUDCROWMOD-HAYMAKER.title"] = "Haymaker",
        ["LOUDCROWMOD-HAYMAKER.description"] = $"Deal {{Damage:diff}} damage. Apply 1 {LoudCrowTextStyles.Vulnerable} to yourself.",
        ["LOUDCROWMOD-TUMBLEWEED_DASH.title"] = "Tumbleweed Dash",
        ["LOUDCROWMOD-TUMBLEWEED_DASH.description"] = $"Deal {{Damage:diff}} damage twice. If this attack will consume a {LoudCrowTextStyles.Bullet}, increase its damage by {{ConsumedBonusDamage}}.",
        ["LOUDCROWMOD-CROW_CALL.title"] = "Crow Call",
        ["LOUDCROWMOD-CROW_CALL.description"] = "Play a random card from your Exhaust pile.",
        ["LOUDCROWMOD-FIRE_BREATHING_GUN.title"] = "Fire Breathing Gun",
        ["LOUDCROWMOD-FIRE_BREATHING_GUN.description"] = $"Deal {{Damage:diff}} damage. Add 2 {LoudCrowTextStyles.Heat} into your Discard Pile.",
        ["LOUDCROWMOD-RICOCHET_SHOT.title"] = "Ricochet Shot",
        ["LOUDCROWMOD-RICOCHET_SHOT.description"] = "Deal {Damage:diff} damage. Deal {BounceDamage:diff} damage to a random enemy.",
        ["LOUDCROWMOD-LOADED_SHOT.title"] = "Loaded Shot",
        ["LOUDCROWMOD-LOADED_SHOT.description"] = "Deal 6 damage. Gain 1 Bullet. Draw 1 card.",
        ["LOUDCROWMOD-RAIN_OF_BULLETS.title"] = "Rain of Bullets",
        ["LOUDCROWMOD-RAIN_OF_BULLETS.description"] = $"Attack random enemies once for each {LoudCrowTextStyles.Bullet} you have. Deal {{Damage:diff}} damage per shot.",
        ["LOUDCROWMOD-READJUSTMENT.title"] = "Readjustment",
        ["LOUDCROWMOD-READJUSTMENT.description"] = $"Gain {{Block:diff}} {LoudCrowTextStyles.Block}. Gain {{BulletCount}} {LoudCrowTextStyles.Bullets}.",
        ["LOUDCROWMOD-QUICK_RELOAD.title"] = "Quick Reload",
        ["LOUDCROWMOD-QUICK_RELOAD.description"] = $"Gain {{BulletCount}} random {LoudCrowTextStyles.Bullets}. Draw 2 cards.",
        ["LOUDCROWMOD-AIMED_SHOT.title"] = "Aimed Shot",
        ["LOUDCROWMOD-AIMED_SHOT.description"] = $"Deal {{Damage:diff}} damage. +{{ConsumedBulletDamage}} damage per {LoudCrowTextStyles.Bullet} consumed by attacks this turn.",
        ["LOUDCROWMOD-PREPARATION.title"] = "Preparation",
        ["LOUDCROWMOD-PREPARATION.description"] = $"Gain {{Block:diff}} {LoudCrowTextStyles.Block}. Gain {{BlockPerBullet}} additional {LoudCrowTextStyles.Block} for each {LoudCrowTextStyles.Bullet} you have.",
        ["LOUDCROWMOD-OVERHEAT.title"] = "Overheat",
        ["LOUDCROWMOD-OVERHEAT.description"] = $"Draw {{DrawCount}} cards. Add 2 {LoudCrowTextStyles.Heat} into your Discard Pile.",
        ["LOUDCROWMOD-BREAKTHROUGH.title"] = "Heat Breakthrough",
        ["LOUDCROWMOD-BREAKTHROUGH.description"] = $"Gain {{Block:diff}} {LoudCrowTextStyles.Block}. Add 2 {LoudCrowTextStyles.Heat} into your Discard Pile.",
        ["LOUDCROWMOD-LOUD_CROW_BREAKTHROUGH.title"] = "Heat Breakthrough",
        ["LOUDCROWMOD-LOUD_CROW_BREAKTHROUGH.description"] = $"Gain {{Block:diff}} {LoudCrowTextStyles.Block}. Add 2 {LoudCrowTextStyles.Heat} into your Discard Pile.",
        ["LOUDCROWMOD-ARREST.title"] = "Arrest",
        ["LOUDCROWMOD-ARREST.description"] = $"Apply {{Weak}} {LoudCrowTextStyles.Weak}. Reduce {LoudCrowTextStyles.Strength} by {{StrengthLoss}}.",
        ["LOUDCROWMOD-ACE_IN_THE_HOLE.title"] = "Ace in the Hole",
        ["LOUDCROWMOD-ACE_IN_THE_HOLE.description"] = $"Deal {{Damage:diff}} damage. Damage increases by {{DamagePerBullet}} for each {LoudCrowTextStyles.Bullet} you have.",
        ["LOUDCROWMOD-ACE_IN_THE_HOLE_SAFE.description"] = "Deal 4 damage. Damage increases by 2 for each Bullet you have.",
        ["LOUDCROWMOD-ACE_IN_THE_HOLE_SAFE_UPG.description"] = "Deal 4 damage. Damage increases by 3 for each Bullet you have.",
        ["LOUDCROWMOD-SCROUNGE.title"] = "Scrounge",
        ["LOUDCROWMOD-SCROUNGE.description"] = $"Gain {{EnergyGain}} Energy. Draw {{BaseDraw}} card. {LoudCrowTextStyles.MagicBullet}: gain {{MagicBulletEnergy}} additional Energy and draw {{MagicBulletDraw}} additional cards.",
        ["LOUDCROWMOD-GHOST_BULLET.title"] = "\uC720\uB839\uD0C4\uD658",
        ["LOUDCROWMOD-GHOST_BULLET.description"] = $"Gain random {LoudCrowTextStyles.Bullets} equal to the number of {LoudCrowTextStyles.Bullets} consumed by attacks this turn.",
        ["LOUDCROWMOD-GRAVE_ROBBING.title"] = "도굴",
        ["LOUDCROWMOD-GRAVE_ROBBING.description"] = "소멸된 카드 더미에서 {ReturnCount}장을 선택해 버린 카드 더미에 넣습니다.",
        ["LOUDCROWMOD-GRAVE_ROBBING.selectionScreenPrompt"] = "버린 카드 더미로 옮길 카드를 선택하세요.",
        ["LOUDCROWMOD-TEST_SETUP.title"] = "테스트 준비",
        ["LOUDCROWMOD-TEST_SETUP.description"] = "이번 전투 동안 다른 모든 카드를 강화합니다. 에너지를 3 얻고 카드를 3장 뽑습니다.",
        ["LOUDCROWMOD-SPEC_SPECIAL_02.title"] = "악조건 대응",
        ["LOUDCROWMOD-SPEC_SPECIAL_02.description"] = $"Gain {{Block:diff}} {LoudCrowTextStyles.Block}. If you have a negative effect, gain {{BonusBlock}} additional {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-GHOST_CLOAK.title"] = "유령 망토",
        ["LOUDCROWMOD-GHOST_CLOAK.description"] = $"Gain {LoudCrowTextStyles.Block} equal to the {LoudCrowTextStyles.Block} you gained last turn.",
        ["LOUDCROWMOD-SPEC_SPECIAL_04.title"] = "재정비",
        ["LOUDCROWMOD-SPEC_SPECIAL_04.description"] = $"Gain 1 Energy. Gain {{BulletCount}} random {LoudCrowTextStyles.Bullets}.",
        ["LOUDCROWMOD-SPEC_SPECIAL_07.title"] = "독한 추진",
        ["LOUDCROWMOD-SPEC_SPECIAL_07.description"] = $"Gain {{WeakAmount}} {LoudCrowTextStyles.Weak}. Gain 1 Energy. Draw {{DrawCount}} card.",
        ["LOUDCROWMOD-SPEC_SPECIAL_10.title"] = "오기",
        ["LOUDCROWMOD-SPEC_SPECIAL_10.description"] = $"Whenever you gain a negative effect, gain {{BulletGain}} {LoudCrowTextStyles.Bullets}.",
        ["LOUDCROWMOD-PERSEVERANCE.title"] = "인내",
        ["LOUDCROWMOD-PERSEVERANCE.description"] = $"At the start of your turn, if you have a negative effect, gain {{BlockGain}} {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-INCINERATION.title"] = "Incineration",
        ["LOUDCROWMOD-INCINERATION.description"] = $"Exhaust 1 card. Gain {{EnergyGain}} Energy.",
        ["LOUDCROWMOD-HEAT_CIRCULATION.title"] = "Heat Circulation",
        ["LOUDCROWMOD-HEAT_CIRCULATION.description"] = $"Whenever you draw {LoudCrowTextStyles.Heat}, draw {{DrawCount}} card.",
        ["LOUDCROWMOD-IGNITION_ENGRAVING.title"] = "격발 각인",
        ["LOUDCROWMOD-IGNITION_ENGRAVING.description"] = $"매 턴 처음으로 사용하는 {LoudCrowTextStyles.Bullet} {{BulletCount}}발이 2배의 효과를 지닙니다.",
        ["LOUDCROWMOD-SPEC_SPECIAL_05.title"] = "가열 사격",
        ["LOUDCROWMOD-SPEC_SPECIAL_05.description"] = $"Deal {{Damage:diff}} damage. If {LoudCrowTextStyles.Heat} is in your hand, deal {{HeatBonusDamage}} more damage.",
        ["LOUDCROWMOD-SPEC_SPECIAL_08.title"] = "열기 순환",
        ["LOUDCROWMOD-SPEC_SPECIAL_08.description"] = $"Whenever {LoudCrowTextStyles.Heat} Exhausts, gain {{BlockGain}} {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-MASTER_OF_CHAOS.title"] = "Master of Chaos",
        ["LOUDCROWMOD-MASTER_OF_CHAOS.description"] = $"Whenever you consume a {LoudCrowTextStyles.Bullet}, gain {{BlockGain}} {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-HIGH_CALIBER_BULLET.title"] = "High-Caliber Bullet",
        ["LOUDCROWMOD-HIGH_CALIBER_BULLET.description"] = $"Load {LoudCrowTextStyles.HighCaliberBullet}.",
        ["LOUDCROWMOD-CAW_CAW.title"] = "Caw Caw!",
        ["LOUDCROWMOD-CAW_CAW.description"] = $"Apply {{Vulnerable}} {LoudCrowTextStyles.Vulnerable} and {{Weak}} {LoudCrowTextStyles.Weak} to ALL enemies. Next turn, all enemies gain {{NextTurnStrength}} {LoudCrowTextStyles.Strength}.",
        ["LOUDCROWMOD-READY_TO_FIRE.title"] = "Ready to Fire",
        ["LOUDCROWMOD-READY_TO_FIRE.description"] = $"Gain {{Block:diff}} {LoudCrowTextStyles.Block}. The next \"Shot\" card costs 0.",
        ["LOUDCROWMOD-TABLE_TURNING.title"] = "Table-Turning",
        ["LOUDCROWMOD-TABLE_TURNING.description"] = "Discard your hand. Draw {DrawCount} cards. Costs 1 less for each card in your hand.",
        ["LOUDCROWMOD-ARMOR_PIERCING_ROUND.title"] = "Armor-Piercing Round",
        ["LOUDCROWMOD-ARMOR_PIERCING_ROUND.description"] = "Deal {Damage:diff} damage. Ignores Block.",
        ["LOUDCROWMOD-TABLE_SHIELD.title"] = "Table Shield",
        ["LOUDCROWMOD-TABLE_SHIELD.description"] = $"Gain {{Block:diff}} {LoudCrowTextStyles.Block}. Add {{PlankCount}} [b]Plank Piece[/b] to your hand.",
        ["LOUDCROWMOD-CROW_FORM.title"] = "Crow Form",
        ["LOUDCROWMOD-CROW_FORM.description"] = "At the start of each turn, choose 1 of 3 cards from your Exhaust pile. It costs 0 this turn and is added to your hand.",
        ["LOUDCROWMOD-RECOVERY.title"] = "Recovery",
        ["LOUDCROWMOD-RECOVERY.description"] = "Choose 2 cards from your Exhaust pile and add them to your hand. They gain Ethereal and Exhaust.",
        ["LOUDCROWMOD-RECOVERY.selectionScreenPrompt"] = "Choose cards to recover.",
        ["LOUDCROWMOD-HEAT_RELEASE.title"] = "Heat Release",
        ["LOUDCROWMOD-HEAT_RELEASE.description"] = $"Transform all {LoudCrowTextStyles.Heat} into [b]Release[/b].",
        ["LOUDCROWMOD-SPEC_RARE_01.title"] = "열광",
        ["LOUDCROWMOD-SPEC_RARE_01.description"] = $"Whenever you create {LoudCrowTextStyles.Heat}, gain {{VigorGain}} Vigor. Add 2 {LoudCrowTextStyles.Heat} into your Discard Pile.",
        ["LOUDCROWMOD-SHINY.title"] = "Shiny!",
        ["LOUDCROWMOD-SHINY.description"] = "Choose 1 of 3 cards from your Exhaust pile and add it to your hand.",
        ["LOUDCROWMOD-SHINY.selectionScreenPrompt"] = "Choose a card to add to your hand.",
        ["LOUDCROWMOD-MAGIC_BULLET_MARKSMAN.title"] = "마탄의 사수",
        ["LOUDCROWMOD-MAGIC_BULLET_MARKSMAN.description"] = "내 턴 시작 시, 마탄을 1 얻습니다.",
        ["LOUDCROWMOD-JUSTICE_APOSTLE.title"] = "Justice Apostle",
        ["LOUDCROWMOD-JUSTICE_APOSTLE.description"] = $"Gain {{BulletCount}} {LoudCrowTextStyles.Bullets}. Whenever you play an Attack this turn, gain 1 {LoudCrowTextStyles.Bullet}.",
        ["LOUDCROWMOD-CROW_HERALD.title"] = "Crow Herald",
        ["LOUDCROWMOD-CROW_HERALD.description"] = "Choose {ChoiceCount} Attack or Skill card from your draw pile. Add it to your hand. It gains Exhaust.",
        ["LOUDCROWMOD-CROW_HERALD.selectionScreenPrompt"] = "Choose cards to herald.",
        ["LOUDCROWMOD-ONSLAUGHT.title"] = "Onslaught",
        ["LOUDCROWMOD-ONSLAUGHT.description"] = $"Deal {{Damage:diff}} damage. Gain {{Block:diff}} {LoudCrowTextStyles.Block}. Gain {{BulletCount}} {LoudCrowTextStyles.Bullets}. Gain {{EnergyGain}} Energy. Draw {{DrawCount}} card.",
        ["LOUDCROWMOD-AMMO_DEPOT.title"] = "Ammo Depot",
        ["LOUDCROWMOD-AMMO_DEPOT.description"] = $"At the start of each turn, gain {{BulletCount}} random {LoudCrowTextStyles.Bullets}.",
        ["LOUDCROWMOD-SPEC_RARE_05.title"] = "예열",
        ["LOUDCROWMOD-SPEC_RARE_05.description"] = $"At the start of your turn, gain 1 Energy and add 1 {LoudCrowTextStyles.Heat} into your Discard Pile.",
        ["LOUDCROWMOD-SPEC_SPECIAL_01.title"] = "탄막 난사",
        ["LOUDCROWMOD-SPEC_SPECIAL_01.description"] = $"Deal {{Damage:diff}} damage to ALL enemies. {LoudCrowTextStyles.MagicBullet}: Consume up to 3 {LoudCrowTextStyles.Bullets} and deal {{DamagePerConsumedBullet}} damage to ALL enemies per {LoudCrowTextStyles.Bullet} consumed.",
        ["LOUDCROWMOD-SPEC_SPECIAL_03.title"] = "연발 사격",
        ["LOUDCROWMOD-SPEC_SPECIAL_03.description"] = $"Deal {{Damage:diff}} damage X times. {LoudCrowTextStyles.MagicBullet}: Attack 2 additional times.",
        ["LOUDCROWMOD-SPEC_COMMON_01.title"] = "쌍발 사격",
        ["LOUDCROWMOD-SPEC_COMMON_01.description"] = $"Lose 2 {LoudCrowTextStyles.Bullets}. Deal {{Damage:diff}} damage.",
        ["LOUDCROWMOD-FOCUSED_FIRE.title"] = "집중 사격",
        ["LOUDCROWMOD-FOCUSED_FIRE.description"] = $"Deal {{Damage:diff}} damage. Consume up to 3 {LoudCrowTextStyles.Bullets}.",
        ["LOUDCROWMOD-SPEC_RARE_04.title"] = "마탄 장전",
        ["LOUDCROWMOD-SPEC_RARE_04.description"] = "마탄을 1 얻습니다.",
        ["LOUDCROWMOD-SPEC_RARE_06.title"] = "오발 장전",
        ["LOUDCROWMOD-SPEC_RARE_06.description"] = $"Gain {{InitialBullets}} random {LoudCrowTextStyles.Bullets}. At the start of your turn, load 1 Misfire.",
        ["LOUDCROWMOD-SPEC_RARE_08.title"] = "전탄 발사",
        ["LOUDCROWMOD-SPEC_RARE_08.description"] = $"Can only be played if you have at least 5 {LoudCrowTextStyles.Bullets}. Deal {{Damage:diff}} damage. Consume 5 {LoudCrowTextStyles.Bullets}.",
        ["LOUDCROWMOD-BOMBARDMENT_MAGIC_BULLET.title"] = "포격류 마탄",
        ["LOUDCROWMOD-BOMBARDMENT_MAGIC_BULLET.description"] = $"Deal {{Damage:diff}} damage. At the end of your turn, if your hand is empty, play this from your Exhaust pile.",
        ["LOUDCROWMOD-MALICE.title"] = "Malice",
        ["LOUDCROWMOD-MALICE.description"] = $"Deal {{Damage:diff}} damage. Remove Artifact from the target. Deal double damage to Minions.",
        ["LOUDCROWMOD-SPEC_RARE_02.title"] = "잔향 사격",
        ["LOUDCROWMOD-SPEC_RARE_02.description"] = $"Deal {{Damage:diff}} damage. This turn, while this is in your Discard pile, whenever you play another Attack, play this on a random enemy.",
        ["LOUDCROWMOD-VOLLEY_FIRE.title"] = "일제 사격",
        ["LOUDCROWMOD-VOLLEY_FIRE.description"] = $"Deal {{Damage:diff}} damage. Play all other Shot cards in your hand.",
        ["LOUDCROWMOD-SPEC_SPECIAL_09.title"] = "앙심",
        ["LOUDCROWMOD-SPEC_SPECIAL_09.description"] = $"Deal {{Damage:diff}} damage. If you have a negative effect, attack 1 additional time.",
        ["LOUDCROWMOD-SPEC_SPECIAL_06.title"] = "연쇄 방아쇠",
        ["LOUDCROWMOD-SPEC_SPECIAL_06.description"] = "피해 {Damage:diff}를 줍니다. 사용 시, 이 카드에 재사용 1을 부여합니다.",
        ["LOUDCROWMOD-NO_GUARD.title"] = "노 가드",
        ["LOUDCROWMOD-NO_GUARD.description"] = $"Lose all current {LoudCrowTextStyles.Block}. Deal damage to ALL enemies equal to the {LoudCrowTextStyles.Block} lost.",
        ["LOUDCROWMOD-DUCK.title"] = "Duck",
        ["LOUDCROWMOD-DUCK.description"] = $"Gain {{Block:diff}} {LoudCrowTextStyles.Block}. {LoudCrowTextStyles.MagicBullet}: Gain {{MagicBulletBlock:diff}} additional {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-OVERHEAT_SHOT.title"] = "Overheat Shot",
        ["LOUDCROWMOD-OVERHEAT_SHOT.description"] = $"Deal {{Damage:diff}} damage. Add 1 {LoudCrowTextStyles.Heat} into your Discard Pile.",
        ["LOUDCROWMOD-RAPID_FIRE.title"] = "Rapid Fire",
        ["LOUDCROWMOD-RAPID_FIRE.description"] = $"Deal {{Damage:diff}} damage 2 times. {LoudCrowTextStyles.MagicBullet}: Attack 1 additional time.",
        ["LOUDCROWMOD-PISTOL_WHIP.title"] = "Pistol Whip",
        ["LOUDCROWMOD-PISTOL_WHIP.description"] = $"Can only be played if you have no {LoudCrowTextStyles.Bullets}. Deal {{Damage:diff}} damage.",
        ["LOUDCROWMOD-SCRAM.title"] = "Scram",
        ["LOUDCROWMOD-SCRAM.description"] = $"Lose 1 {LoudCrowTextStyles.Bullet}. Gain {{Block:diff}} {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-CROW_FEATHER.title"] = "Crow Feather",
        ["LOUDCROWMOD-CROW_FEATHER.description"] = $"Gain {{StrengthGain}} temporary {LoudCrowTextStyles.Strength}.",
        ["LOUDCROWMOD-HIGH_NOON.title"] = "High Noon",
        ["LOUDCROWMOD-HIGH_NOON.description"] = $"Apply {{Vulnerable}} {LoudCrowTextStyles.Vulnerable} to ALL enemies.",
        ["LOUDCROWMOD-SPARE_AMMO.title"] = "Spare Ammo",
        ["LOUDCROWMOD-SPARE_AMMO.description"] = $"Gain {{BulletCount}} random {LoudCrowTextStyles.Bullets}.",
        ["LOUDCROWMOD-MOST_WANTED.title"] = "Most Wanted",
        ["LOUDCROWMOD-MOST_WANTED.description"] = $"Apply Most Wanted to an enemy. {LoudCrowTextStyles.Bullet} effects on that target are doubled this turn. Gain {{BulletGain}} {LoudCrowTextStyles.Bullet}.",
        ["LOUDCROWMOD-FULL_BURST.title"] = "풀 버스트",
        ["LOUDCROWMOD-FULL_BURST.description"] = $"Discard 1 card. Deal {{Damage:diff}} damage to ALL enemies. {LoudCrowTextStyles.MagicBullet}: deal {{MagicBulletDamage:diff}} damage instead. Gain {LoudCrowTextStyles.Block} equal to unblocked damage dealt.",
        ["LOUDCROWMOD-MAKESHIFT_MEASURE.title"] = "Makeshift Measure",
        ["LOUDCROWMOD-MAKESHIFT_MEASURE.description"] = $"Gain {{Block:diff}} {LoudCrowTextStyles.Block}. Next turn, gain 1 Frail.",
        ["LOUDCROWMOD-AMMO_SUPPLY.title"] = "Ammo Supply",
        ["LOUDCROWMOD-AMMO_SUPPLY.description"] = $"Gain {{BulletCount}} random {LoudCrowTextStyles.Bullets} X times.",
        ["LOUDCROWMOD-RELOADING.title"] = "Reloading",
        ["LOUDCROWMOD-RELOADING.description"] = $"Discard 2 cards. Gain {{BulletCount}} {LoudCrowTextStyles.Bullets}. Draw 3 cards.",
        ["LOUDCROWMOD-OUTLAW.title"] = "Outlaw",
        ["LOUDCROWMOD-OUTLAW.description"] = "This turn, all cards cost 1 less. Played cards Exhaust. At the end of turn, Exhaust your hand.",
        ["LOUDCROWMOD-HEAT.title"] = "Heat",
        ["LOUDCROWMOD-HEAT.description"] = "Take 2 damage.",
        ["LOUDCROWMOD-RELEASE.title"] = "Release",
        ["LOUDCROWMOD-RELEASE.description"] = "Deal {Damage:diff} damage. Draw {DrawCount} card.",
        ["LOUDCROWMOD-PLANK_PIECE.title"] = "Plank Piece",
        ["LOUDCROWMOD-PLANK_PIECE.description"] = $"Gain {{Block:diff}} {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-MAGIC_BULLET.title"] = "Magic Bullet",
        ["LOUDCROWMOD-MAGIC_BULLET.description"] = "If your hand is empty when the card resolves, use the listed Magic Bullet value instead of its normal effect.",
        ["LOUDCROWMOD-BULLET.title"] = "Bullet",
        ["LOUDCROWMOD-BULLET.description"] = LoudCrowKeywordText.BulletFullDescription,
        ["LOUDCROWMOD-HIGH_CALIBER_ROUND.title"] = "\uB300\uAD6C\uACBD \uD0C4\uD658",
        ["LOUDCROWMOD-HIGH_CALIBER_ROUND.description"] = "\uB2E4\uC74C \uACF5\uACA9\uC5D0 \uC18C\uBAA8\uB420 \uB54C, \uADF8 \uACF5\uACA9\uC758 \uD53C\uD574\uB97C 2\uBC30\uB85C \uB9CC\uB4ED\uB2C8\uB2E4.",
        ["LOUDCROWMOD-SPEC_RARE_03.title"] = "초토화",
        ["LOUDCROWMOD-SPEC_RARE_03.description"] = $"Exhaust all other cards in your hand. Deal {{Damage:diff}} damage to ALL enemies. {LoudCrowTextStyles.MagicBullet}: increase this damage by {{DamagePerExhaustedCard}} per card Exhausted this way.",
        ["LOUDCROWMOD-PERFECT_FINISH.title"] = "Perfect Finish",
        ["LOUDCROWMOD-PERFECT_FINISH.description"] = $"At the end of your turn, if your hand is empty, gain {{Block:diff}} {LoudCrowTextStyles.Block}.",
        ["LOUDCROWMOD-SELF_IMMOLATION.title"] = "분신",
        ["LOUDCROWMOD-SELF_IMMOLATION.description"] = "취약, 약화, 손상 중 무작위 2개를 얻습니다. 에너지를 {EnergyGain} 얻습니다. 카드를 {DrawCount}장 뽑습니다. 소멸.",
        ["LOUDCROWMOD-DULLNESS.title"] = "둔감",
        ["LOUDCROWMOD-DULLNESS.description"] = "취약/약화/손상의 효과가 절반으로 감소합니다.",
        ["LOUDCROWMOD-INTOXICATION.title"] = "도취",
        ["LOUDCROWMOD-INTOXICATION.description"] = $"Whenever you consume a {LoudCrowTextStyles.Bullet}, gain 1 temporary {LoudCrowTextStyles.Strength} this turn.",
        ["LOUDCROWMOD-SPEC_RARE_09.title"] = "건져 올리기",
        ["LOUDCROWMOD-SPEC_RARE_09.description"] = "Add {ReturnCount} random cards from your Exhaust pile to your hand.",
        ["LOUDCROWMOD-SPEC_RARE_10.title"] = "한발 더",
        ["LOUDCROWMOD-SPEC_RARE_10.description"] = "At the start of your turn, draw 1 card. It gains Exhaust.",
    };

    static CardLocalization()
    {
        Entries["LOUDCROWMOD-NO_GUARD.title"] = "No Guard";

        AddDefaultRuntimeAliases();

        // Do not override the base game's BREAKTHROUGH localization.
        // Loud Crow's card uses the renamed runtime entry LOUD_CROW_BREAKTHROUGH.
        Entries.Remove("BREAKTHROUGH.title");
        Entries.Remove("BREAKTHROUGH.description");

        AddRuntimeAliases("GHOST_BULLET", "LOUDCROWMOD-GHOST_BULLET");
        AddRuntimeAliases("GRAVE_ROBBING", "LOUDCROWMOD-GRAVE_ROBBING");
        AddRuntimeAliases("GHOST_CLOAK", "LOUDCROWMOD-GHOST_CLOAK");
        AddRuntimeAliases("PERSEVERANCE", "LOUDCROWMOD-PERSEVERANCE");
        AddRuntimeAliases("IGNITION_ENGRAVING", "LOUDCROWMOD-IGNITION_ENGRAVING");
        AddRuntimeAliases("FOCUSED_FIRE", "LOUDCROWMOD-FOCUSED_FIRE");
        AddRuntimeAliases("BOMBARDMENT_MAGIC_BULLET", "LOUDCROWMOD-BOMBARDMENT_MAGIC_BULLET");
        AddRuntimeAliases("MALICE", "LOUDCROWMOD-MALICE");
        AddRuntimeAliases("VOLLEY_FIRE", "LOUDCROWMOD-VOLLEY_FIRE");
        AddRuntimeAliases("NO_GUARD", "LOUDCROWMOD-NO_GUARD");
        AddRuntimeAliases("PERFECT_FINISH", "LOUDCROWMOD-PERFECT_FINISH");
        AddRuntimeAliases("SELF_IMMOLATION", "LOUDCROWMOD-SELF_IMMOLATION");
        AddRuntimeAliases("DULLNESS", "LOUDCROWMOD-DULLNESS");
        AddRuntimeAliases("INTOXICATION", "LOUDCROWMOD-INTOXICATION");
        AddRuntimeAliases("FULL_BURST", "LOUDCROWMOD-FULL_BURST");

        AddRuntimeAliases("SPEC_COMMON01", "LOUDCROWMOD-SPEC_COMMON_01");
        AddRuntimeAliases("SPEC_SPECIAL01", "LOUDCROWMOD-SPEC_SPECIAL_01");
        AddRuntimeAliases("SPEC_SPECIAL02", "LOUDCROWMOD-SPEC_SPECIAL_02");
        AddRuntimeAliases("SPEC_SPECIAL03", "LOUDCROWMOD-SPEC_SPECIAL_03");
        AddRuntimeAliases("SPEC_SPECIAL04", "LOUDCROWMOD-SPEC_SPECIAL_04");
        AddRuntimeAliases("SPEC_SPECIAL05", "LOUDCROWMOD-SPEC_SPECIAL_05");
        AddRuntimeAliases("SPEC_SPECIAL06", "LOUDCROWMOD-SPEC_SPECIAL_06");
        AddRuntimeAliases("SPEC_SPECIAL07", "LOUDCROWMOD-SPEC_SPECIAL_07");
        AddRuntimeAliases("SPEC_SPECIAL08", "LOUDCROWMOD-SPEC_SPECIAL_08");
        AddRuntimeAliases("SPEC_SPECIAL09", "LOUDCROWMOD-SPEC_SPECIAL_09");
        AddRuntimeAliases("SPEC_SPECIAL10", "LOUDCROWMOD-SPEC_SPECIAL_10");
        AddRuntimeAliases("SPEC_RARE01", "LOUDCROWMOD-SPEC_RARE_01");
        AddRuntimeAliases("SPEC_RARE02", "LOUDCROWMOD-SPEC_RARE_02");
        AddRuntimeAliases("SPEC_RARE03", "LOUDCROWMOD-SPEC_RARE_03");
        AddRuntimeAliases("SPEC_RARE04", "LOUDCROWMOD-SPEC_RARE_04");
        AddRuntimeAliases("SPEC_RARE05", "LOUDCROWMOD-SPEC_RARE_05");
        AddRuntimeAliases("SPEC_RARE06", "LOUDCROWMOD-SPEC_RARE_06");
        AddRuntimeAliases("SPEC_RARE08", "LOUDCROWMOD-SPEC_RARE_08");
        AddRuntimeAliases("SPEC_RARE09", "LOUDCROWMOD-SPEC_RARE_09");
        AddRuntimeAliases("SPEC_RARE10", "LOUDCROWMOD-SPEC_RARE_10");
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
            {
                Entries[$"{normalized}{suffix}"] = pair.Value;
                Entries[$"LOUDCROWMOD-{normalized}{suffix}"] = pair.Value;
            }
        }
    }

    private static string NormalizeSpecRuntimeEntry(string runtimeEntry)
    {
        if (runtimeEntry.StartsWith("SPEC_RARE_"))
            return runtimeEntry.Replace("SPEC_RARE_", "SPEC_RARE");
        if (runtimeEntry.StartsWith("SPEC_SPECIAL_"))
            return runtimeEntry.Replace("SPEC_SPECIAL_", "SPEC_SPECIAL");
        if (runtimeEntry.StartsWith("SPEC_COMMON_"))
            return runtimeEntry.Replace("SPEC_COMMON_", "SPEC_COMMON");

        return runtimeEntry;
    }

    private static void AddRuntimeAliases(string runtimeEntry, string canonicalPrefix)
    {
        CopyIfPresent($"{canonicalPrefix}.title", $"{runtimeEntry}.title");
        CopyIfPresent($"{canonicalPrefix}.description", $"{runtimeEntry}.description");
        CopyIfPresent($"{canonicalPrefix}.selectionScreenPrompt", $"{runtimeEntry}.selectionScreenPrompt");
    }

    private static void CopyIfPresent(string sourceKey, string aliasKey)
    {
        if (!Entries.TryGetValue(sourceKey, out var value))
            return;

        Entries[aliasKey] = value;
    }
}


