using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using LoudCrowMod.Cards;
using System.Collections.Generic;
using System.Reflection;
using LoudCrowMod.Relics;

namespace LoudCrowMod;

public class LoudCrowCardPool : CustomCardPoolModel
{
    public override bool IsColorless => false;
    public override bool IsShared => false;
    public override string Title => "loudcrow";
    public override string EnergyColorName => "ironclad";
    public override Color DeckEntryCardColor => new Color("A0522DFF");
    public override Color ShaderColor => new Color("A0522DFF");

    protected override CardModel[] GenerateAllCards() =>
    [
        ModelDb.Card<Strike>(),
        ModelDb.Card<Defend>(),
        ModelDb.Card<LoudCrowBurst>(),
        ModelDb.Card<Haymaker>(),
        ModelDb.Card<TumbleweedDash>(),
        ModelDb.Card<CrowCall>(),
        ModelDb.Card<FireBreathingGun>(),
        ModelDb.Card<RicochetShot>(),
        ModelDb.Card<Midnight>(),
        ModelDb.Card<LoadedShot>(),
        ModelDb.Card<Readjustment>(),
        ModelDb.Card<QuickReload>(),
        ModelDb.Card<AimedShot>(),
        ModelDb.Card<RainOfBullets>(),
        ModelDb.Card<Preparation>(),
        ModelDb.Card<Overheat>(),
        ModelDb.Card<LoudCrowBreakthrough>(),
        ModelDb.Card<Arrest>(),
        ModelDb.Card<AceInTheHole>(),
        ModelDb.Card<Scrounge>(),
        ModelDb.Card<GhostBullet>(),
        ModelDb.Card<GraveRobbing>(),
        ModelDb.Card<SpecSpecial02>(),
        ModelDb.Card<GhostCloak>(),
        ModelDb.Card<SpecSpecial04>(),
        ModelDb.Card<SpecSpecial07>(),
        ModelDb.Card<SpecSpecial10>(),
        ModelDb.Card<Perseverance>(),
        ModelDb.Card<Incineration>(),
        ModelDb.Card<HeatCirculation>(),
        ModelDb.Card<IgnitionEngraving>(),
        ModelDb.Card<SpecSpecial05>(),
        ModelDb.Card<SpecSpecial08>(),
        ModelDb.Card<MasterOfChaos>(),
        ModelDb.Card<HighCaliberBullet>(),
        ModelDb.Card<CawCaw>(),
        ModelDb.Card<ReadyToFire>(),
        ModelDb.Card<TableTurning>(),
        ModelDb.Card<ArmorPiercingRound>(),
        ModelDb.Card<TableShield>(),
        ModelDb.Card<CrowForm>(),
        ModelDb.Card<Recovery>(),
        ModelDb.Card<HeatRelease>(),
        ModelDb.Card<TestSetup>(),
        ModelDb.Card<SpecRare01>(),
        ModelDb.Card<Shiny>(),
        ModelDb.Card<MagicBulletMarksman>(),
        ModelDb.Card<JusticeApostle>(),
        ModelDb.Card<CrowHerald>(),
        ModelDb.Card<Onslaught>(),
        ModelDb.Card<AmmoDepot>(),
        ModelDb.Card<SpecRare05>(),
        ModelDb.Card<SpecSpecial01>(),
        ModelDb.Card<SpecSpecial03>(),
        ModelDb.Card<SpecCommon01>(),
        ModelDb.Card<FocusedFire>(),
        ModelDb.Card<SpecRare04>(),
        ModelDb.Card<SpecRare06>(),
        ModelDb.Card<SpecRare08>(),
        ModelDb.Card<BombardmentMagicBullet>(),
        ModelDb.Card<Malice>(),
        ModelDb.Card<SpecRare02>(),
        ModelDb.Card<VolleyFire>(),
        ModelDb.Card<SpecSpecial09>(),
        ModelDb.Card<SpecSpecial06>(),
        ModelDb.Card<NoGuard>(),
        ModelDb.Card<SpecRare03>(),
        ModelDb.Card<PerfectFinish>(),
        ModelDb.Card<SelfImmolation>(),
        ModelDb.Card<Dullness>(),
        ModelDb.Card<Intoxication>(),
        ModelDb.Card<SpecRare09>(),
        ModelDb.Card<SpecRare10>(),
        ModelDb.Card<Duck>(),
        ModelDb.Card<OverheatShot>(),
        ModelDb.Card<RapidFire>(),
        ModelDb.Card<PistolWhip>(),
        ModelDb.Card<Scram>(),
        ModelDb.Card<CrowFeather>(),
        ModelDb.Card<HighNoon>(),
        ModelDb.Card<SpareAmmo>(),
        ModelDb.Card<MostWanted>(),
        ModelDb.Card<FullBurst>(),
        ModelDb.Card<MakeshiftMeasure>(),
        ModelDb.Card<AmmoSupply>(),
        ModelDb.Card<Reloading>(),
        ModelDb.Card<Outlaw>(),
    ];
}

public class LoudCrowCharacter : CustomCharacterModel
{
    private enum TestDeckPreset
    {
        CoreShots,
        HeatTest,
        MagicBulletTest,
        RetainTest,
        RelicTest,
        BatchAUpdateTest,
        BatchBResourceTest,
        BatchCHeatTest,
        BatchDCombatTest,
        BatchECombatTest,
        BatchFCombatTest,
        AddedCardsTest,
    }

    private enum TestRelicPreset
    {
        Default,
        ShootingTarget,
        TornContract,
        ExpressMail,
        EchoBullet,
        Bandolier,
    }

    private enum TestPotionPreset
    {
        Default,
        AllLoudCrowPotions,
    }

    private const TestDeckPreset CurrentDeckPreset = TestDeckPreset.CoreShots;
    private const TestRelicPreset CurrentRelicPreset = TestRelicPreset.Default;
    private const TestPotionPreset CurrentPotionPreset = TestPotionPreset.Default;

    private static CardModel BuiltInCard(string entry)
    {
        var assembly = typeof(CardModel).Assembly;

        foreach (var typeName in new[] { "SaveUtil", "MegaCrit.Sts2.Core.Saves.SaveUtil" })
        {
            var saveUtilType = assembly.GetType(typeName);
            if (saveUtilType == null)
                continue;

            foreach (var method in saveUtilType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (method.Name != "CardOrDeprecated" || method.ReturnType != typeof(CardModel))
                    continue;

                var parameters = method.GetParameters();
                if (parameters.Length != 1)
                    continue;

                var arg = CreateIdArgument(parameters[0].ParameterType, entry);
                if (arg == null)
                    continue;

                if (method.Invoke(null, new[] { arg }) is CardModel card)
                    return card;
            }
        }

        foreach (var method in typeof(ModelDb).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (method.Name != "GetById" || !method.IsGenericMethodDefinition)
                continue;

            var generic = method.MakeGenericMethod(typeof(CardModel));
            var parameters = generic.GetParameters();
            if (parameters.Length != 1)
                continue;

            var arg = CreateIdArgument(parameters[0].ParameterType, entry);
            if (arg == null)
                continue;

            if (generic.Invoke(null, new[] { arg }) is CardModel card)
                return card;
        }

        throw new MissingMemberException($"Could not resolve built-in card '{entry}'.");
    }

    private static object? CreateIdArgument(Type parameterType, string entry)
    {
        if (parameterType == typeof(string))
            return entry;

        foreach (var method in parameterType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if ((method.Name == "Parse" || method.Name == "FromString" || method.Name == "op_Implicit") &&
                method.GetParameters().Length == 1 &&
                method.GetParameters()[0].ParameterType == typeof(string))
            {
                try
                {
                    return method.Invoke(null, new object[] { entry });
                }
                catch
                {
                }
            }
        }

        foreach (var ctor in parameterType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var args = TryBuildConstructorArgs(ctor.GetParameters(), entry);
            if (args == null)
                continue;

            try
            {
                return ctor.Invoke(args);
            }
            catch
            {
            }
        }

        object? instance = null;
        try
        {
            instance = Activator.CreateInstance(parameterType, nonPublic: true);
        }
        catch
        {
        }

        if (instance == null)
            return null;

        foreach (var propertyName in new[] { "Entry", "Id" })
        {
            var property = parameterType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property?.CanWrite == true && property.PropertyType == typeof(string))
            {
                property.SetValue(instance, entry);
                return instance;
            }
        }

        foreach (var fieldName in new[] { "Entry", "Id", "_entry", "_id" })
        {
            var field = parameterType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field?.FieldType == typeof(string))
            {
                field.SetValue(instance, entry);
                return instance;
            }
        }

        return null;
    }

    private static object[]? TryBuildConstructorArgs(ParameterInfo[] parameters, string entry)
    {
        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
            return new object[] { entry };

        if (parameters.Length == 2 &&
            parameters[0].ParameterType == typeof(string) &&
            parameters[1].ParameterType == typeof(string))
            return new object[] { entry, string.Empty };

        if (parameters.Length == 2 &&
            parameters[0].ParameterType == typeof(string) &&
            parameters[1].ParameterType == typeof(bool))
            return new object[] { entry, false };

        return null;
    }

    public override CharacterGender Gender => CharacterGender.Masculine;
    public override Color NameColor => new Color("A0522DFF");
    public override int StartingHp => 80;
    public override int StartingGold => 99;

    public override CardPoolModel CardPool =>
        (CardPoolModel)ModelDb.CardPool<LoudCrowCardPool>();

    public override RelicPoolModel RelicPool =>
        (RelicPoolModel)ModelDb.RelicPool<LoudCrowRelicPool>();

    public override PotionPoolModel PotionPool =>
        (PotionPoolModel)ModelDb.PotionPool<LoudCrowPotionPool>();

    public override IEnumerable<CardModel> StartingDeck => BuildStartingDeck();

    public override IReadOnlyList<RelicModel> StartingRelics => BuildStartingRelics();

    public override IReadOnlyList<PotionModel> StartingPotions => BuildStartingPotions();

    public override List<string> GetArchitectAttackVfx() => new List<string>
    {
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter",
    };

    public override string? CustomCharacterSelectBg =>
        LoudCrowCharacterAssetPaths.CharacterSelectBgPath;
    public override string? CustomCharacterSelectIconPath =>
        LoudCrowCharacterAssetPaths.CharacterSelectIconPath;
    public override string? CustomCharacterSelectLockedIconPath =>
        LoudCrowCharacterAssetPaths.CharacterSelectLockedIconPath;
    public override string? CustomCharacterSelectTransitionPath =>
        LoudCrowCharacterAssetPaths.CharacterSelectTransitionPath;
    public override string? CustomVisualPath =>
        LoudCrowCharacterAssetPaths.VisualsPath;
    public override string? CustomIconTexturePath =>
        LoudCrowCharacterAssetPaths.IconTexturePath;
    public override string? CustomIconPath =>
        LoudCrowCharacterAssetPaths.IconPath;
    public override string? CustomEnergyCounterPath =>
        LoudCrowCharacterAssetPaths.EnergyCounterPath;
    public override string? CustomArmPointingTexturePath =>
        "res://images/ui/hands/multiplayer_hand_ironclad_point.png";
    public override string? CustomArmRockTexturePath =>
        "res://images/ui/hands/multiplayer_hand_ironclad_rock.png";
    public override string? CustomArmPaperTexturePath =>
        "res://images/ui/hands/multiplayer_hand_ironclad_paper.png";
    public override string? CustomArmScissorsTexturePath =>
        "res://images/ui/hands/multiplayer_hand_ironclad_scissors.png";
    public override string? CustomAttackSfx =>
        "event:/sfx/characters/ironclad/ironclad_attack";
    public override string? CustomCastSfx =>
        "event:/sfx/characters/ironclad/ironclad_cast";
    public override string? CustomDeathSfx =>
        "event:/sfx/characters/ironclad/ironclad_die";
    public override string? CustomRestSiteAnimPath =>
        LoudCrowCharacterAssetPaths.RestSiteAnimPath;
    public override string? CustomMerchantAnimPath =>
        LoudCrowCharacterAssetPaths.MerchantAnimPath;
    public override string? CustomMapMarkerPath =>
        LoudCrowCharacterAssetPaths.MapMarkerPath;
    public override string? CustomTrailPath =>
        LoudCrowCharacterAssetPaths.TrailPath;

    private static IReadOnlyList<CardModel> BuildStartingDeck()
    {
        return CurrentDeckPreset switch
        {
            TestDeckPreset.CoreShots => new List<CardModel>
            {
                ModelDb.Card<Strike>(),
                ModelDb.Card<Strike>(),
                ModelDb.Card<LoudCrowBurst>(),
                ModelDb.Card<LoadedShot>(),
                ModelDb.Card<RicochetShot>(),
                ModelDb.Card<OverheatShot>(),
                ModelDb.Card<HighCaliberBullet>(),
                ModelDb.Card<MostWanted>(),
                ModelDb.Card<SpareAmmo>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
            },
            TestDeckPreset.HeatTest => new List<CardModel>
            {
                ModelDb.Card<Strike>(),
                ModelDb.Card<Strike>(),
                ModelDb.Card<OverheatShot>(),
                ModelDb.Card<FireBreathingGun>(),
                ModelDb.Card<Overheat>(),
                ModelDb.Card<LoudCrowBreakthrough>(),
                ModelDb.Card<HeatRelease>(),
                ModelDb.Card<SpareAmmo>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Midnight>(),
            },
            TestDeckPreset.MagicBulletTest => new List<CardModel>
            {
                ModelDb.Card<Strike>(),
                ModelDb.Card<Strike>(),
                ModelDb.Card<Duck>(),
                ModelDb.Card<Midnight>(),
                ModelDb.Card<Duck>(),
                ModelDb.Card<RapidFire>(),
                ModelDb.Card<Scrounge>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<SpareAmmo>(),
            },
            TestDeckPreset.RetainTest => new List<CardModel>
            {
                ModelDb.Card<Strike>(),
                ModelDb.Card<Strike>(),
                ModelDb.Card<TableShield>(),
                ModelDb.Card<GhostBullet>(),
                ModelDb.Card<LoadedShot>(),
                ModelDb.Card<RicochetShot>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<SpareAmmo>(),
                ModelDb.Card<Midnight>(),
            },
            TestDeckPreset.RelicTest => new List<CardModel>
            {
                ModelDb.Card<Strike>(),
                ModelDb.Card<Strike>(),
                ModelDb.Card<LoadedShot>(),
                ModelDb.Card<RicochetShot>(),
                ModelDb.Card<OverheatShot>(),
                ModelDb.Card<MostWanted>(),
                ModelDb.Card<HighCaliberBullet>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<SpareAmmo>(),
            },
            TestDeckPreset.BatchAUpdateTest => new List<CardModel>
            {
                ModelDb.Card<TestSetup>(),
                ModelDb.Card<TumbleweedDash>(),
                ModelDb.Card<CrowCall>(),
                ModelDb.Card<Scrounge>(),
                ModelDb.Card<CrowHerald>(),
                ModelDb.Card<CrowFeather>(),
                ModelDb.Card<MostWanted>(),
                ModelDb.Card<JusticeApostle>(),
                ModelDb.Card<Outlaw>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Strike>(),
                ModelDb.Card<Strike>(),
            },
            TestDeckPreset.BatchBResourceTest => new List<CardModel>
            {
                ModelDb.Card<TestSetup>(),
                ModelDb.Card<SpecSpecial02>(),
                ModelDb.Card<GhostCloak>(),
                ModelDb.Card<SpecSpecial04>(),
                ModelDb.Card<SpecSpecial07>(),
                ModelDb.Card<SpecSpecial10>(),
                ModelDb.Card<Perseverance>(),
                ModelDb.Card<Haymaker>(),
                ModelDb.Card<MakeshiftMeasure>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Strike>(),
                ModelDb.Card<Strike>(),
            },
            TestDeckPreset.BatchCHeatTest => new List<CardModel>
            {
                ModelDb.Card<TestSetup>(),
                ModelDb.Card<SpecSpecial05>(),
                ModelDb.Card<SpecSpecial08>(),
                ModelDb.Card<SpecRare01>(),
                ModelDb.Card<SpecRare05>(),
                ModelDb.Card<HeatRelease>(),
                ModelDb.Card<Overheat>(),
                ModelDb.Card<LoudCrowBreakthrough>(),
                ModelDb.Card<OverheatShot>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Strike>(),
                ModelDb.Card<Strike>(),
            },
            TestDeckPreset.BatchDCombatTest => new List<CardModel>
            {
                ModelDb.Card<TestSetup>(),
                ModelDb.Card<SpecSpecial01>(),
                ModelDb.Card<SpecSpecial03>(),
                ModelDb.Card<SpecCommon01>(),
                ModelDb.Card<FocusedFire>(),
                ModelDb.Card<SpecRare04>(),
                ModelDb.Card<SpecRare06>(),
                ModelDb.Card<SpecRare08>(),
                ModelDb.Card<BombardmentMagicBullet>(),
                ModelDb.Card<SpareAmmo>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Strike>(),
            },
            TestDeckPreset.BatchECombatTest => new List<CardModel>
            {
                ModelDb.Card<TestSetup>(),
                ModelDb.Card<Malice>(),
                ModelDb.Card<SpecRare02>(),
                ModelDb.Card<SpecRare02>(),
                ModelDb.Card<VolleyFire>(),
                ModelDb.Card<SpecSpecial09>(),
                ModelDb.Card<SpecSpecial06>(),
                ModelDb.Card<NoGuard>(),
                ModelDb.Card<LoadedShot>(),
                ModelDb.Card<RicochetShot>(),
                ModelDb.Card<Haymaker>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Strike>(),
            },
            TestDeckPreset.BatchFCombatTest => new List<CardModel>
            {
                ModelDb.Card<TestSetup>(),
                ModelDb.Card<SpecRare03>(),
                ModelDb.Card<PerfectFinish>(),
                ModelDb.Card<SpecRare09>(),
                ModelDb.Card<SpecRare10>(),
                ModelDb.Card<Duck>(),
                ModelDb.Card<Midnight>(),
                ModelDb.Card<Shiny>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Strike>(),
                ModelDb.Card<Strike>(),
            },
            TestDeckPreset.AddedCardsTest => new List<CardModel>
            {
                ModelDb.Card<TestSetup>(),
                ModelDb.Card<GraveRobbing>(),
                ModelDb.Card<Intoxication>(),
                ModelDb.Card<IgnitionEngraving>(),
                ModelDb.Card<SelfImmolation>(),
                ModelDb.Card<Dullness>(),
                ModelDb.Card<FullBurst>(),
                ModelDb.Card<Shiny>(),
                ModelDb.Card<SpecRare09>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Strike>(),
                ModelDb.Card<Strike>(),
            },
            _ => new List<CardModel>
            {
                ModelDb.Card<Strike>(),
                ModelDb.Card<Strike>(),
                ModelDb.Card<LoudCrowBurst>(),
                ModelDb.Card<LoadedShot>(),
                ModelDb.Card<RicochetShot>(),
                ModelDb.Card<OverheatShot>(),
                ModelDb.Card<HighCaliberBullet>(),
                ModelDb.Card<MostWanted>(),
                ModelDb.Card<SpareAmmo>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
                ModelDb.Card<Defend>(),
            },
        };
    }

    private static IReadOnlyList<PotionModel> BuildStartingPotions()
    {
        return CurrentPotionPreset switch
        {
            TestPotionPreset.AllLoudCrowPotions => new List<PotionModel>
            {
                ModelDb.Potion<BulletPotion>(),
                ModelDb.Potion<LavaPotion>(),
                ModelDb.Potion<MagicBulletPotion>(),
            },
            _ => [],
        };
    }

    private static IReadOnlyList<RelicModel> BuildStartingRelics()
    {
        return CurrentRelicPreset switch
        {
            TestRelicPreset.Default => new List<RelicModel>
            {
                ModelDb.Relic<BulletPouch>(),
            },
            TestRelicPreset.ShootingTarget => new List<RelicModel>
            {
                ModelDb.Relic<BulletPouch>(),
                ModelDb.Relic<ShootingTarget>(),
            },
            TestRelicPreset.TornContract => new List<RelicModel>
            {
                ModelDb.Relic<BulletPouch>(),
                ModelDb.Relic<TornContract>(),
            },
            TestRelicPreset.ExpressMail => new List<RelicModel>
            {
                ModelDb.Relic<BulletPouch>(),
                ModelDb.Relic<ExpressMail>(),
            },
            TestRelicPreset.EchoBullet => new List<RelicModel>
            {
                ModelDb.Relic<BulletPouch>(),
                ModelDb.Relic<EchoBullet>(),
            },
            TestRelicPreset.Bandolier => new List<RelicModel>
            {
                ModelDb.Relic<BulletPouch>(),
                ModelDb.Relic<Bandolier>(),
            },
            _ => new List<RelicModel>
            {
                ModelDb.Relic<BulletPouch>(),
                ModelDb.Relic<ExpressMail>(),
            },
        };
    }
}



