using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Players;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LoudCrowMod.Powers;
using LoudCrowMod.Relics;

namespace LoudCrowMod.Cards;

public abstract class LoudCrowCardModel : BaseLib.Abstracts.CustomCardModel
{
    protected LoudCrowCardModel(
        int baseCost,
        CardType type,
        CardRarity rarity,
        TargetType target,
        bool showInCardLibrary = true,
        bool autoAdd = false)
        : base(baseCost, type, rarity, target, showInCardLibrary, autoAdd)
    {
    }
}

internal static class LoudCrowCardLogic
{
    private static readonly MethodInfo? SetThisTurnOrUntilPlayedMethod =
        typeof(CardEnergyCost).GetMethod(
            "SetThisTurnOrUntilPlayed",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            [typeof(int), typeof(bool)],
            null);
    private static readonly HashSet<CardModel> OutlawTemporaryEtherealCards = [];
    private static readonly HashSet<CardModel> OutlawTemporaryExhaustCards = [];

    private static readonly HashSet<string> ShotCardEntries =
    [
        "RICOCHET_SHOT",
        "LOADED_SHOT",
        "AIMED_SHOT",
        "OVERHEAT_SHOT",
    ];

    private static string NormalizeEntry(string? entry)
    {
        if (string.IsNullOrWhiteSpace(entry))
            return string.Empty;

        const string prefix = "LOUDCROWMOD-";
        return entry.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)
            ? entry[prefix.Length..]
            : entry;
    }

    public static IEnumerable<IHoverTip> GetHeatHoverTips()
    {
        return LoudCrowKeywordHoverTips.HeatCardOnly;
    }

    public static bool IsShotCard(CardModel? card)
    {
        return ShotCardEntries.Contains(NormalizeEntry(card?.Id?.Entry));
    }

    public static bool IsTableTurning(CardModel? card)
    {
        return NormalizeEntry(card?.Id?.Entry) == "TABLE_TURNING";
    }

    public static int GetTableTurningCost(CardModel card)
    {
        var hand = card.Owner?.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        int handCount = hand?.Cards.Count ?? 0;
        return System.Math.Max(0, 5 - handCount);
    }

    public static decimal? GetCreatureBlock(Creature creature)
    {
        foreach (string propertyName in new[] { "Block", "CurrentBlock" })
        {
            var property = creature.GetType().GetProperty(propertyName);
            if (property?.CanRead == true)
            {
                object? value = property.GetValue(creature);
                if (value != null)
                    return System.Convert.ToDecimal(value);
            }
        }

        foreach (string fieldName in new[] { "Block", "CurrentBlock", "_block", "_currentBlock" })
        {
            var field = creature.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                object? value = field.GetValue(creature);
                if (value != null)
                    return System.Convert.ToDecimal(value);
            }
        }

        return null;
    }

    public static void SetCreatureBlock(Creature creature, decimal amount)
    {
        foreach (string propertyName in new[] { "Block", "CurrentBlock" })
        {
            var property = creature.GetType().GetProperty(propertyName);
            if (property?.CanWrite == true)
            {
                property.SetValue(creature, System.Convert.ChangeType(amount, property.PropertyType));
                return;
            }
        }

        foreach (string fieldName in new[] { "Block", "CurrentBlock", "_block", "_currentBlock" })
        {
            var field = creature.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(creature, System.Convert.ChangeType(amount, field.FieldType));
                return;
            }
        }
    }

    public static bool HasMagicBullet(Player owner)
    {
        if (LoudCrowTurnEffects.HasLoadedMagicBullet(owner))
            return true;

        var hand = owner.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        return hand == null || !hand.Cards.Any();
    }

    public static bool IsMagicBulletPrimed(CardModel card)
    {
        var owner = card.Owner;
        if (owner == null)
            return false;

        if (LoudCrowTurnEffects.HasLoadedMagicBullet(owner))
            return true;

        var hand = owner.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        return hand != null && hand.Cards.Count == 1 && hand.Cards.Contains(card);
    }

    public static decimal EstimateUnblockedDamage(
        CardModel card,
        Creature target,
        decimal baseDamage)
    {
        decimal modifiedDamage = Hook.ModifyDamage(
            card.Owner.RunState,
            card.CombatState,
            target,
            card.Owner.Creature,
            baseDamage,
            ValueProp.Move,
            card,
            ModifyDamageHookType.All,
            CardPreviewMode.Normal,
            out IEnumerable<AbstractModel> _);

        decimal currentBlock = GetCreatureBlock(target) ?? 0M;
        return System.Math.Max(0M, modifiedDamage - currentBlock);
    }

    public static async System.Threading.Tasks.Task QueueRandomBullets(
        Player owner, CardModel? cardSource, int count)
    {
        var rng = owner.RunState.Rng.CombatTargets;
        for (int i = 0; i < count; i++)
        {
            var bullet = rng.NextItem(BulletSystem.AllBulletTypes);
            await BulletSystem.QueueBullet(owner.Creature, bullet, owner.Creature, cardSource);
        }
    }

    public static async System.Threading.Tasks.Task ConsumeMagicBullet(
        Player owner,
        PlayerChoiceContext? choiceContext = null,
        CardModel? sourceCard = null)
    {
        LoudCrowTurnEffects.ConsumeLoadedMagicBullet(owner);
        await LoudCrowRelicEffects.OnMagicBulletTriggered(choiceContext, owner, sourceCard);
    }

    public static int BulletCount(Player owner) => BulletSystem.GetBulletCount(owner.Creature);

    public static async Task<int> ConsumeAttackBullets(
        PlayerChoiceContext? choiceContext,
        Creature creature,
        int count)
    {
        return await BulletSystem.ConsumeBulletsForAttack(choiceContext, creature, count);
    }

    public static async System.Threading.Tasks.Task AddHeatToDiscard(
        Player owner, CardModel? sourceCard, int count)
    {
        if (count <= 0)
            return;

        if (owner.Creature.CombatState == null)
            return;

        await CardPileCmd.AddToCombatAndPreview<Heat>(
            owner.Creature,
            PileType.Discard,
            count,
            true,
            CardPilePosition.Top);

        string sheriffBadgeId = ModelDb.GetId(typeof(SheriffBadge)).Entry;
        var sheriffBadge = owner.Relics.FirstOrDefault(relic => relic.Id.Entry == sheriffBadgeId);
        if (sheriffBadge != null)
        {
            sheriffBadge.Flash();
            await CreatureCmd.GainBlock(owner.Creature, 3M * count, ValueProp.Move, null);
        }

        await LoudCrowTurnEffects.OnHeatCreated(owner, count, sourceCard);
    }

    public static async System.Threading.Tasks.Task AddPlankPiecesToHand(
        Player owner, int count, bool upgraded = false)
    {
        var combatState = owner.Creature.CombatState;
        if (combatState == null || count <= 0)
            return;

        List<CardModel> created = [];
        for (int i = 0; i < count; i++)
        {
            var plankPiece = combatState.CreateCard<PlankPiece>(owner);
            if (plankPiece == null)
                continue;

            BindGeneratedCardPool(plankPiece);

            if (upgraded && plankPiece.IsUpgradable)
            {
                plankPiece.UpgradeInternal();
                plankPiece.FinalizeUpgradeInternal();
            }

            created.Add(plankPiece);
        }

        if (created.Count > 0)
            await CardPileCmd.AddGeneratedCardsToCombat(created, PileType.Hand, true, CardPilePosition.Top);
    }

    public static async System.Threading.Tasks.Task TransformHeatToRelease(
        Player owner, bool upgraded)
    {
        var combatPiles = owner.Piles.Where(p => p.IsCombatPile).ToList();
        foreach (var pile in combatPiles)
        {
            var heats = pile.Cards.OfType<Heat>().ToList();
            foreach (var heat in heats)
            {
                var release = owner.Creature.CombatState?.CreateCard<Release>(owner);
                if (release == null)
                    continue;

                BindGeneratedCardPool(release);

                if (upgraded)
                {
                    release.UpgradeInternal();
                    release.FinalizeUpgradeInternal();
                }

                await CardCmd.Transform(heat, release);
            }
        }

        RefreshHandVisuals(owner);
    }

    public static void RefreshHandVisuals(Player owner)
    {
        var hand = owner.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null)
            return;

        foreach (var card in hand.Cards)
        {
            var node = NCard.FindOnTable(card);
            if (node == null)
                continue;

            node.UpdateVisuals(node.DisplayingPile, CardPreviewMode.Normal);
        }
    }

    public static void RefreshHandVisualsDeferred(Player owner)
    {
        TaskHelper.RunSafely(RefreshHandVisualsDeferredInternal(owner));
    }

    private static async System.Threading.Tasks.Task RefreshHandVisualsDeferredInternal(Player owner)
    {
        await System.Threading.Tasks.Task.Yield();
        RefreshHandVisuals(owner);
    }

    private static void BindGeneratedCardPool(CardModel card)
    {
        try
        {
            if (global::LoudCrowMod.LoudCrowCardPoolResolver.TryResolve(card.GetType(), out var pool))
                global::LoudCrowMod.LoudCrowCardPoolResolver.Bind(card, pool);
        }
        catch
        {
        }
    }

    public static Player? GetOwningPlayer(Creature creature)
    {
        return creature.CombatState?.Players.FirstOrDefault(player => player.Creature == creature);
    }

    public static void ReduceCardCostThisTurn(CardModel card, int amount)
    {
        if (amount <= 0)
            return;

        try
        {
            int currentCost = card.EnergyCost.GetWithModifiers(CostModifiers.All);
            int reducedCost = System.Math.Max(0, currentCost - amount);
            SetThisTurnOrUntilPlayedMethod?.Invoke(card.EnergyCost, [reducedCost, true]);
            card.InvokeEnergyCostChanged();
        }
        catch
        {
        }
    }

    public static void ApplyOutlawToCard(CardModel card)
    {
        if (!card.ExhaustOnNextPlay)
        {
            card.ExhaustOnNextPlay = true;
            OutlawTemporaryExhaustCards.Add(card);
        }
        ReduceCardCostThisTurn(card, 1);
        if (!card.Keywords.Contains(CardKeyword.Ethereal))
        {
            card.AddKeyword(CardKeyword.Ethereal);
            OutlawTemporaryEtherealCards.Add(card);
        }
    }

    public static void ApplyOutlawToHand(Player player)
    {
        var hand = player.Piles.FirstOrDefault(pile => pile.Type == PileType.Hand);
        if (hand == null)
            return;

        foreach (var card in hand.Cards)
            ApplyOutlawToCard(card);

        RefreshHandVisuals(player);
    }

    public static void CleanupOutlawCard(CardModel card, bool preserveExhaust = false)
    {
        if (OutlawTemporaryEtherealCards.Remove(card))
            card.RemoveKeyword(CardKeyword.Ethereal);

        if (!preserveExhaust && OutlawTemporaryExhaustCards.Remove(card))
            card.ExhaustOnNextPlay = false;
    }

    public static void ResetOutlawTemporaryCards()
    {
        foreach (var card in OutlawTemporaryEtherealCards.ToList())
        {
            try
            {
                card.RemoveKeyword(CardKeyword.Ethereal);
            }
            catch
            {
            }
        }

        foreach (var card in OutlawTemporaryExhaustCards.ToList())
        {
            try
            {
                card.ExhaustOnNextPlay = false;
            }
            catch
            {
            }
        }

        OutlawTemporaryEtherealCards.Clear();
        OutlawTemporaryExhaustCards.Clear();
    }

    public static bool IsOutlawTemporaryEthereal(CardModel? card)
    {
        return card != null && OutlawTemporaryEtherealCards.Contains(card);
    }

    public static CardModel? CreateRandomExhaustClone(Player owner)
    {
        var exhaustPile = owner.Piles.FirstOrDefault(pile => pile.Type == PileType.Exhaust);
        var exhaustCards = exhaustPile?.Cards.ToList() ?? [];
        if (exhaustCards.Count == 0)
            return null;

        int index = owner.RunState.Rng.CombatCardSelection.NextInt(exhaustCards.Count);
        var selectedCard = exhaustCards[index];
        var clone = selectedCard.CreateClone();
        clone.ExhaustOnNextPlay = true;
        return clone;
    }

    public static List<CardModel> TakeRandomExhaustCards(Player owner, int count)
    {
        if (count <= 0)
            return [];

        var exhaustPile = owner.Piles.FirstOrDefault(pile => pile.Type == PileType.Exhaust);
        var pool = exhaustPile?.Cards.ToList() ?? [];
        if (pool.Count == 0)
            return [];

        var results = new List<CardModel>();
        while (results.Count < count && pool.Count > 0)
        {
            int index = owner.RunState.Rng.CombatCardSelection.NextInt(pool.Count);
            results.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return results;
    }
}

public class ShotDamageVar : DamageVar
{
    public ShotDamageVar(decimal damage, ValueProp props)
        : base(damage, props)
    {
    }

    public ShotDamageVar(string name, decimal damage, ValueProp props)
        : base(name, damage, props)
    {
    }

    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        decimal originalDamage = this.BaseValue;
        EnchantmentModel? enchantment = card.Enchantment;
        if (enchantment != null)
        {
            decimal enchantedDamage = originalDamage + enchantment.EnchantDamageAdditive(originalDamage, this.Props);
            originalDamage = enchantedDamage * enchantment.EnchantDamageMultiplicative(enchantedDamage, this.Props);
            if (!card.IsEnchantmentPreview)
                this.EnchantedValue = originalDamage;
        }

        if (runGlobalHooks)
        {
            originalDamage = Hook.ModifyDamage(
                card.Owner.RunState,
                card.CombatState,
                target,
                card.Owner.Creature,
                originalDamage,
                this.Props,
                card,
                ModifyDamageHookType.All,
                previewMode,
                out IEnumerable<AbstractModel> _);
        }

        this.PreviewValue = originalDamage;
    }
}

// ???? ????????????????????????????????????????????????????????????????????????????????????????????????

public class Strike() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Strike };
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(6M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3M);
}

public class Defend() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => new() { CardTag.Defend };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(5M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3M);
}

public class LoudCrowBurst() : LoudCrowCardModel(
    0, CardType.Attack, CardRarity.Basic, TargetType.AllEnemies, true, false)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.MagicBulletOnly;

    protected override bool ShouldGlowGoldInternal => LoudCrowCardLogic.IsMagicBulletPrimed(this);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6M, ValueProp.Move),
        new DamageVar("EmptyHandDamage", 9M, ValueProp.Move),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        var hand = Owner.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        bool hadCards = hand != null && hand.Cards.Any();

        if (hadCards)
        {
            var card = (await CardSelectCmd.FromHandForDiscard(
                choiceContext, Owner,
                new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1),
                null, this)).FirstOrDefault();
            if (card != null)
                await CardCmd.Discard(choiceContext, card);
        }

        bool handEmptyNow = hand == null || !hand.Cards.Any();
        bool magicBullet = handEmptyNow || LoudCrowTurnEffects.HasLoadedMagicBullet(Owner);
        decimal damage = magicBullet
            ? DynamicVars["EmptyHandDamage"].BaseValue
            : DynamicVars.Damage.BaseValue;
        if (magicBullet)
            await LoudCrowCardLogic.ConsumeMagicBullet(Owner, choiceContext, this);

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3M);
        DynamicVars["EmptyHandDamage"].UpgradeValueBy(3M);
    }
}


// ???? ???占쎈턄嶺뚮∥??醫꽷?????????????????????????????????????????????????????????????????????????????????????????????

public class Haymaker() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.VulnerableOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(16M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
        await PowerCmd.Apply<VulnerablePower>(Owner.Creature, 1M, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3M);
}

// ???? ????占쎈뾼???????????????????????????????????????????????????????????????????????????????????????????????

public class TumbleweedDash() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new TumbleweedDashDamageVar(4M, ValueProp.Move),
        new DynamicVar("ConsumedBonusDamage", 2M),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        bool willConsumeBullet = BulletSystem.GetBulletCount(Owner.Creature) > 0;
        decimal damage = DynamicVars.Damage.BaseValue;
        if (willConsumeBullet)
            damage += DynamicVars["ConsumedBonusDamage"].BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this).Targeting(play.Target!).WithHitCount(2).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1M);
        DynamicVars["ConsumedBonusDamage"].UpgradeValueBy(1M);
    }
}

public class TumbleweedDashDamageVar(decimal damage, ValueProp props) : DamageVar(damage, props)
{
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        base.UpdateCardPreview(card, previewMode, target, runGlobalHooks);

        if (card is not TumbleweedDash tumbleweedDash)
            return;

        if (BulletSystem.GetBulletCount(card.Owner.Creature) <= 0)
            return;

        if (!tumbleweedDash.DynamicVars.TryGetValue("ConsumedBonusDamage", out DynamicVar? bonus))
            return;

        PreviewValue += bonus.BaseValue;
    }
}

// ???? 濚밸Ŧ?占썲퐲琉몃돥? ??怨쀫꼶 ????????????????????????????????????????????????????????????????????????????????????????????

public class CrowCall() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        var randomClone = LoudCrowCardLogic.CreateRandomExhaustClone(Owner);
        if (randomClone == null)
            return;

        await CardCmd.AutoPlay(choiceContext, randomClone, null);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, false));
        InvokeEnergyCostChanged();
    }
}

// ???? ????占쎈츊????????????????????????????????????????????????????????????????????????????????????????????????
public class FireBreathingGun() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowCardLogic.GetHeatHoverTips();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(12M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
        await LoudCrowCardLogic.AddHeatToDiscard(Owner, this, 2);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3M);
}
// ???? 亦껋꼶梨띈キ??瑜곷엾 ????????????????????????????????????????????????????????????????????????????????????????????
public class RicochetShot() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ShotDamageVar(7M, ValueProp.Move),
        new ShotDamageVar("BounceDamage", 2M, ValueProp.Move),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target).Execute(choiceContext);

        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        var livingOpponents = combatState.GetOpponentsOf(Owner.Creature)
            .Where(creature => creature.IsAlive)
            .ToList();
        if (livingOpponents.Count == 0)
            return;

        var otherOpponents = livingOpponents
            .Where(creature => creature != play.Target)
            .ToList();
        var bouncePool = otherOpponents.Count > 0 ? otherOpponents : livingOpponents;
        var randomTarget = Owner.RunState.Rng.CombatTargets.NextItem(bouncePool);
        if (randomTarget == null)
            return;

        await DamageCmd.Attack(DynamicVars["BounceDamage"].BaseValue)
            .FromCard(this).Targeting(randomTarget).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1M);
        DynamicVars["BounceDamage"].UpgradeValueBy(1M);
    }
}

public class Midnight() : LoudCrowCardModel(
    2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal =>
        LoudCrowCardLogic.IsMagicBulletPrimed(this);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.MagicBulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(16M, ValueProp.Move),
        new DamageVar("MagicBulletDamage", 32M, ValueProp.Move),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        bool magicBullet = LoudCrowCardLogic.HasMagicBullet(Owner);
        decimal damage = magicBullet
            ? DynamicVars["MagicBulletDamage"].BaseValue
            : DynamicVars.Damage.BaseValue;
        if (magicBullet)
            await LoudCrowCardLogic.ConsumeMagicBullet(Owner, choiceContext, this);

        await DamageCmd.Attack(damage)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5M);
        DynamicVars["MagicBulletDamage"].UpgradeValueBy(10M);
    }
}
// ???? ????????????????????????????????????????????????????????????????????????????????????????????????????
public class LoadedShot() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ShotDamageVar(6M, ValueProp.Move),
        new DynamicVar("BulletCount", 1M),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);

        await LoudCrowCardLogic.QueueRandomBullets(Owner, this, (int)DynamicVars["BulletCount"].BaseValue);
        await CardPileCmd.Draw(choiceContext, 1M, Owner, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3M);
        DynamicVars["BulletCount"].UpgradeValueBy(1M);
    }
}
// ???? ????占쎈쑏?????????????????????????????????????????????????????????????????????????????????????????????
public class Readjustment() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8M, ValueProp.Move),
        new DynamicVar("BulletCount", 2M),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
        await LoudCrowCardLogic.QueueRandomBullets(Owner, this, (int)DynamicVars["BulletCount"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3M);
        DynamicVars["BulletCount"].UpgradeValueBy(1M);
    }
}
// ???? ??占쎈∥占?????????????????????????????????????????????????????????????????????????????????????????????????
public class QuickReload() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BulletCount", 1M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await LoudCrowCardLogic.QueueRandomBullets(Owner, this, (int)DynamicVars["BulletCount"].BaseValue);
        await CardPileCmd.Draw(choiceContext, 2M, Owner, false);
    }

    protected override void OnUpgrade() => DynamicVars["BulletCount"].UpgradeValueBy(1M);
}
// ???? ?占쎄퀗?? ????????????????????????????????????????????????????????????????????????????????????????????????
public class AimedShot() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ShotDamageVar(5M, ValueProp.Move),
        new DynamicVar("ConsumedBulletDamage", 3M),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        int consumed = BulletSystem.GetAttackBulletsConsumedThisTurn(Owner.Creature);
        decimal damage = DynamicVars.Damage.BaseValue +
            (DynamicVars["ConsumedBulletDamage"].BaseValue * consumed);

        await DamageCmd.Attack(damage)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2M);
        DynamicVars["ConsumedBulletDamage"].UpgradeValueBy(1M);
    }
}
// ???? ???占쎄덧?洹먮뜄??????????????????????????????????????????????????????????????????????????????????????????????
public class RainOfBullets() : LoudCrowCardModel(
    2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(4M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        int shots = LoudCrowCardLogic.BulletCount(Owner);
        if (shots <= 0)
            return;

        for (int i = 0; i < shots; i++)
        {
            var livingOpponents = combatState.GetOpponentsOf(Owner.Creature)
                .Where(creature => creature.IsAlive)
                .ToList();
            if (livingOpponents.Count == 0)
                break;

            var randomTarget = Owner.RunState.Rng.CombatTargets.NextItem(livingOpponents);
            if (randomTarget == null)
                break;

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this).Targeting(randomTarget).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1M);
}

public class Preparation() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5M, ValueProp.Move),
        new DynamicVar("BlockPerBullet", 1M),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal block = DynamicVars.Block.BaseValue +
            (DynamicVars["BlockPerBullet"].BaseValue * LoudCrowCardLogic.BulletCount(Owner));
        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2M);
        DynamicVars["BlockPerBullet"].UpgradeValueBy(1M);
    }
}

public class Overheat() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowCardLogic.GetHeatHoverTips();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("DrawCount", 2M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars["DrawCount"].BaseValue, Owner, false);
        await LoudCrowCardLogic.AddHeatToDiscard(Owner, this, 2);
    }

    protected override void OnUpgrade() => DynamicVars["DrawCount"].UpgradeValueBy(1M);
}

public class LoudCrowBreakthrough() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowCardLogic.GetHeatHoverTips();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(11M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
        await LoudCrowCardLogic.AddHeatToDiscard(Owner, this, 2);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4M);
}

public class Arrest() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.WeakAndStrengthOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Weak", 2M),
        new DynamicVar("StrengthLoss", 1M),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
            return;

        await PowerCmd.Apply<WeakPower>(play.Target, DynamicVars["Weak"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(play.Target, -DynamicVars["StrengthLoss"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["StrengthLoss"].UpgradeValueBy(1M);
}

public class AceInTheHole() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4M, ValueProp.Move),
        new DynamicVar("DamagePerBullet", 2M),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal totalDamage = DynamicVars.Damage.BaseValue +
            (DynamicVars["DamagePerBullet"].BaseValue * LoudCrowCardLogic.BulletCount(Owner));

        await DamageCmd.Attack(totalDamage)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DamagePerBullet"].UpgradeValueBy(1M);
    }
}

public class Scrounge() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal =>
        LoudCrowCardLogic.IsMagicBulletPrimed(this);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.MagicBulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("EnergyGain", 0M),
        new DynamicVar("BaseDraw", 1M),
        new DynamicVar("MagicBulletEnergy", 2M),
        new DynamicVar("MagicBulletDraw", 2M),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal energyGain = DynamicVars["EnergyGain"].BaseValue;
        decimal drawCount = DynamicVars["BaseDraw"].BaseValue;
        if (LoudCrowCardLogic.HasMagicBullet(Owner))
        {
            energyGain += DynamicVars["MagicBulletEnergy"].BaseValue;
            drawCount += DynamicVars["MagicBulletDraw"].BaseValue;
            await LoudCrowCardLogic.ConsumeMagicBullet(Owner, choiceContext, this);
        }

        if (Owner?.PlayerCombatState == null)
            return;

        if (energyGain > 0)
            Owner.PlayerCombatState.GainEnergy(energyGain);

        if (drawCount > 0)
            await CardPileCmd.Draw(choiceContext, drawCount, Owner, false);
    }

    protected override void OnUpgrade() => DynamicVars["EnergyGain"].UpgradeValueBy(1M);
}

public class GhostBullet() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        IsUpgraded ? [CardKeyword.Retain, CardKeyword.Exhaust] : [CardKeyword.Exhaust];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        int consumed = BulletSystem.GetAttackBulletsConsumedThisTurn(Owner.Creature);
        if (consumed <= 0)
            return;

        await LoudCrowCardLogic.QueueRandomBullets(Owner, this, consumed);
    }

    protected override void OnUpgrade()
    {
        _ = Keywords;
        AddKeyword(CardKeyword.Retain);
    }
}

public class GraveRobbing() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("ReturnCount", 3M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var exhaustPile = Owner.Piles.FirstOrDefault(pile => pile.Type == PileType.Exhaust);
        var exhaustCards = exhaustPile?.Cards.ToList() ?? [];
        int amount = System.Math.Min((int)DynamicVars["ReturnCount"].BaseValue, exhaustCards.Count);
        if (amount <= 0)
            return;

        var selectedCards = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            exhaustCards,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, amount));

        foreach (var selectedCard in selectedCards)
            await CardPileCmd.Add(selectedCard, PileType.Discard, source: this);
    }

    protected override void OnUpgrade() => DynamicVars["ReturnCount"].UpgradeValueBy(1M);
}

public class TestSetup() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Innate, CardKeyword.Exhaust];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combatCards = Owner.Piles
            .Where(pile => pile.Type.IsCombatPile())
            .SelectMany(pile => pile.Cards)
            .Distinct()
            .Where(card => card != this)
            .ToList();

        foreach (var card in combatCards)
        {
            if (!card.IsUpgradable)
                continue;

            card.UpgradeInternal();
            card.FinalizeUpgradeInternal();
        }

        if (Owner.PlayerCombatState != null)
            Owner.PlayerCombatState.GainEnergy(3M);

        await CardPileCmd.Draw(choiceContext, 3M, Owner, false);

        LoudCrowCardLogic.RefreshHandVisuals(Owner);
    }

    protected override void OnUpgrade() { }
}

public class SpecSpecial02() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal =>
        LoudCrowTurnEffects.HasNegativeEffect(Owner.Creature);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7M, ValueProp.Move),
        new BlockVar("BonusBlock", 7M, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);

        if (LoudCrowTurnEffects.HasNegativeEffect(Owner.Creature))
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["BonusBlock"].BaseValue, ValueProp.Move, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3M);
        DynamicVars["BonusBlock"].UpgradeValueBy(3M);
    }
}

public class GhostCloak() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        IsUpgraded ? [CardKeyword.Exhaust, CardKeyword.Retain] : [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int block = LoudCrowTurnEffects.GetPreviousTurnBlockGained(Owner.Creature);
        if (block <= 0)
            return;

        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, play);
    }

    protected override void OnUpgrade()
    {
        _ = Keywords;
        AddKeyword(CardKeyword.Retain);
    }
}

public class SpecSpecial04() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BulletCount", 1M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        Owner.PlayerCombatState?.GainEnergy(1M);
        await LoudCrowCardLogic.QueueRandomBullets(Owner, this, (int)DynamicVars["BulletCount"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["BulletCount"].UpgradeValueBy(2M);
}

public class SpecSpecial07() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.WeakOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("WeakAmount", 1M),
        new DynamicVar("DrawCount", 1M),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<WeakPower>(Owner.Creature, DynamicVars["WeakAmount"].BaseValue, Owner.Creature, this);
        Owner.PlayerCombatState?.GainEnergy(1M);
        await CardPileCmd.Draw(choiceContext, DynamicVars["DrawCount"].BaseValue, Owner, false);
    }

    protected override void OnUpgrade() => DynamicVars["DrawCount"].UpgradeValueBy(1M);
}

public class SpecSpecial10() : LoudCrowCardModel(
    1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BulletGain", 1M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<SpecSpecial10Power>(Owner.Creature, DynamicVars["BulletGain"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["BulletGain"].UpgradeValueBy(1M);
}

public class Perseverance() : LoudCrowCardModel(
    1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BlockGain", 6M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<PerseverancePower>(Owner.Creature, DynamicVars["BlockGain"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["BlockGain"].UpgradeValueBy(2M);
}

public class Incineration() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("EnergyGain", 2M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = Owner.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        int exhaustCount = hand == null ? 0 : System.Math.Min(1, hand.Cards.Count(card => card != this));
        if (exhaustCount > 0)
        {
            var selectedCard = (await CardSelectCmd.FromHandForDiscard(
                choiceContext,
                Owner,
                new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, exhaustCount),
                card => card != this,
                this)).FirstOrDefault();

            if (selectedCard != null)
                await CardCmd.Exhaust(choiceContext, selectedCard);
        }

        if (Owner?.PlayerCombatState == null)
            return;

        Owner.PlayerCombatState.GainEnergy(DynamicVars["EnergyGain"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, false));
        InvokeEnergyCostChanged();
    }
}

public class HeatCirculation() : LoudCrowCardModel(
    1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowCardLogic.GetHeatHoverTips();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("DrawCount", 1M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<HeatCirculationPower>(
            Owner.Creature,
            DynamicVars["DrawCount"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}

public class IgnitionEngraving() : LoudCrowCardModel(
    1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BulletCount", 1M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<IgnitionEngravingPower>(
            Owner.Creature,
            DynamicVars["BulletCount"].BaseValue,
            Owner.Creature,
            this);

        if (Owner.Creature.GetPower(ModelDb.GetId(typeof(IgnitionEngravingPower))) is IgnitionEngravingPower power)
            LoudCrowTurnEffects.SetIgnitionEngravingCapacity(Owner.Creature, power.Amount);
    }

    protected override void OnUpgrade() => DynamicVars["BulletCount"].UpgradeValueBy(1M);
}

public class SpecSpecial05() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowCardLogic.GetHeatHoverTips();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8M, ValueProp.Move),
        new DynamicVar("HeatBonusDamage", 4M),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);

        bool hasHeatInHand = Owner.Piles.FirstOrDefault(pile => pile.Type == PileType.Hand)
            ?.Cards.Any(card => card is Heat) == true;
        if (hasHeatInHand)
        {
            await DamageCmd.Attack(DynamicVars["HeatBonusDamage"].BaseValue)
                .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3M);
        DynamicVars["HeatBonusDamage"].UpgradeValueBy(3M);
    }
}

public class SpecSpecial08() : LoudCrowCardModel(
    1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowCardLogic.GetHeatHoverTips();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BlockGain", 4M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<SpecSpecial08Power>(
            Owner.Creature,
            DynamicVars["BlockGain"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade() => DynamicVars["BlockGain"].UpgradeValueBy(1M);
}

public class MasterOfChaos() : LoudCrowCardModel(
    1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BlockGain", 2M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<MasterOfChaosPower>(
            Owner.Creature,
            DynamicVars["BlockGain"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade() => DynamicVars["BlockGain"].UpgradeValueBy(1M);
}

public class HighCaliberBullet() : LoudCrowCardModel(
    2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletAndHighCaliberBullet;

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await BulletSystem.QueueBulletFront(Owner.Creature, BulletKind.HighCaliber, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 1, false));
        InvokeEnergyCostChanged();
    }
}

public class CawCaw() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.VulnerableWeakAndStrengthOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Vulnerable", 2M),
        new DynamicVar("Weak", 2M),
        new DynamicVar("NextTurnStrength", 1M),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        foreach (var enemy in combatState.GetOpponentsOf(Owner.Creature).Where(creature => creature.IsAlive))
        {
            await PowerCmd.Apply<VulnerablePower>(enemy, DynamicVars["Vulnerable"].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<WeakPower>(enemy, DynamicVars["Weak"].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<CawCawStrengthPower>(enemy, DynamicVars["NextTurnStrength"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Vulnerable"].UpgradeValueBy(1M);
        DynamicVars["Weak"].UpgradeValueBy(1M);
    }
}

public class ReadyToFire() : LoudCrowCardModel(
    2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(12M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
        await PowerCmd.Apply<ReadyToFirePower>(Owner.Creature, 1M, Owner.Creature, this);
        LoudCrowCardLogic.RefreshHandVisuals(Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4M);
}

public class TableTurning() : LoudCrowCardModel(
    5, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("DrawCount", 5M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = Owner.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        var cardsToDiscard = hand?.Cards.Where(card => card != this).ToList() ?? [];
        foreach (var card in cardsToDiscard)
            await CardCmd.Discard(choiceContext, card);

        await CardPileCmd.Draw(choiceContext, DynamicVars["DrawCount"].BaseValue, Owner, false);
    }

    protected override void OnUpgrade() => DynamicVars["DrawCount"].UpgradeValueBy(1M);
}

public class ArmorPiercingRound() : LoudCrowCardModel(
    2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(14M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
            return;

        decimal? originalBlock = LoudCrowCardLogic.GetCreatureBlock(play.Target);
        try
        {
            if (originalBlock.HasValue && originalBlock.Value > 0)
                LoudCrowCardLogic.SetCreatureBlock(play.Target, 0M);

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this).Targeting(play.Target).Execute(choiceContext);
        }
        finally
        {
            if (originalBlock.HasValue && play.Target.IsAlive)
                LoudCrowCardLogic.SetCreatureBlock(play.Target, originalBlock.Value);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4M);
}

public class TableShield() : LoudCrowCardModel(
    2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.PlankPieceOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10M, ValueProp.Move),
        new DynamicVar("PlankCount", 2M),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
        await LoudCrowCardLogic.AddPlankPiecesToHand(Owner, (int)DynamicVars["PlankCount"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["PlankCount"].UpgradeValueBy(1M);
}

public class CrowForm() : LoudCrowCardModel(
    3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        IsUpgraded ? [] : [CardKeyword.Ethereal];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<CrowFormPower>(Owner.Creature, 1M, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}

public class Dullness() : LoudCrowCardModel(
    2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.VulnerableWeakAndFrailOnly;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<DullnessPower>(Owner.Creature, 1M, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 1, false));
        InvokeEnergyCostChanged();
    }
}

public class Intoxication() : LoudCrowCardModel(
    1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletAndStrengthOnly;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<IntoxicationPower>(Owner.Creature, 1M, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}

public class Recovery() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        var exhaustPile = Owner.Piles.FirstOrDefault(p => p.Type == PileType.Exhaust);
        var exhaustCards = exhaustPile?.Cards.ToList() ?? [];
        int amount = System.Math.Min(2, exhaustCards.Count);
        if (amount <= 0)
            return;

        var selectedCards = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            exhaustCards,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, amount));

        foreach (var selectedCard in selectedCards)
        {
            await CardPileCmd.Add(selectedCard, PileType.Hand, source: this);
            selectedCard.AddKeyword(CardKeyword.Ethereal);
            selectedCard.AddKeyword(CardKeyword.Exhaust);
        }
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, false));
        InvokeEnergyCostChanged();
    }
}

public class HeatRelease() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        IsUpgraded
            ? LoudCrowKeywordHoverTips.HeatAndReleaseUpgradedOnly
            : LoudCrowKeywordHoverTips.HeatAndReleaseOnly;

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await LoudCrowCardLogic.TransformHeatToRelease(Owner, IsUpgraded);
    }

    protected override void OnUpgrade() { }
}

public class SpecRare01() : LoudCrowCardModel(
    1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowCardLogic.GetHeatHoverTips();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("VigorGain", 2M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<SpecRare01Power>(
            Owner.Creature,
            DynamicVars["VigorGain"].BaseValue,
            Owner.Creature,
            this);
        await LoudCrowCardLogic.AddHeatToDiscard(Owner, this, 2);
    }

    protected override void OnUpgrade() => DynamicVars["VigorGain"].UpgradeValueBy(1M);
}

public class Shiny() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        var exhaustPile = Owner.Piles.FirstOrDefault(p => p.Type == PileType.Exhaust);
        var exhaustCards = exhaustPile?.Cards.ToList() ?? [];
        if (exhaustCards.Count == 0)
            return;

        List<CardModel> choices;
        if (exhaustCards.Count <= 3)
        {
            choices = exhaustCards;
        }
        else
        {
            choices = [];
            var pool = exhaustCards.ToList();
            for (int i = 0; i < 3 && pool.Count > 0; i++)
            {
                int index = Owner.RunState.Rng.CombatCardSelection.NextInt(pool.Count);
                choices.Add(pool[index]);
                pool.RemoveAt(index);
            }
        }

        var selectedCard = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            choices,
            Owner,
            canSkip: false);
        if (selectedCard == null)
            return;

        if (IsUpgraded && selectedCard.IsUpgradable)
        {
            selectedCard.UpgradeInternal();
            selectedCard.FinalizeUpgradeInternal();
        }

        await CardPileCmd.Add(selectedCard, PileType.Hand, source: this);
    }

    protected override void OnUpgrade() { }
}

public class MagicBulletMarksman() : LoudCrowCardModel(
    3, CardType.Power, CardRarity.Ancient, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.MagicBulletOnly;

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<MagicBulletMarksmanPower>(Owner.Creature, 1M, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 2, false));
        InvokeEnergyCostChanged();
    }
}

public class JusticeApostle() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BulletCount", 2M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await LoudCrowCardLogic.QueueRandomBullets(Owner, this, (int)DynamicVars["BulletCount"].BaseValue);
        await PowerCmd.Apply<JusticeApostlePower>(Owner.Creature, 1M, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, false));
        InvokeEnergyCostChanged();
    }
}

public class CrowHerald() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("ChoiceCount", 1M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        var drawPile = Owner.Piles.FirstOrDefault(pile => pile.Type == PileType.Draw);
        var candidates = drawPile?.Cards
            .Where(card => card.Type is CardType.Attack or CardType.Skill)
            .ToList() ?? [];
        int choiceCount = System.Math.Min((int)DynamicVars["ChoiceCount"].BaseValue, candidates.Count);
        if (choiceCount <= 0)
            return;

        var selectedCards = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            candidates,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, choiceCount));

        foreach (var selectedCard in selectedCards)
        {
            await CardPileCmd.Add(selectedCard, PileType.Hand, source: this);
            selectedCard.AddKeyword(CardKeyword.Exhaust);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ChoiceCount"].UpgradeValueBy(1M);
    }
}

public class Onslaught() : LoudCrowCardModel(
    0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6M, ValueProp.Move),
        new BlockVar(6M, ValueProp.Move),
        new DynamicVar("BulletCount", 2M),
        new DynamicVar("EnergyGain", 1M),
        new DynamicVar("DrawCount", 1M),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
        await LoudCrowCardLogic.QueueRandomBullets(Owner, this, (int)DynamicVars["BulletCount"].BaseValue);

        if (Owner?.PlayerCombatState == null)
            return;
        Owner.PlayerCombatState.GainEnergy(DynamicVars["EnergyGain"].BaseValue);
        await CardPileCmd.Draw(choiceContext, DynamicVars["DrawCount"].BaseValue, Owner, false);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}

public class AmmoDepot() : LoudCrowCardModel(
    3, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BulletCount", 2M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<AmmoDepotPower>(
            Owner.Creature,
            DynamicVars["BulletCount"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 2, false));
        InvokeEnergyCostChanged();
    }
}

public class SpecRare05() : LoudCrowCardModel(
    2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowCardLogic.GetHeatHoverTips();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<SpecRare05Power>(Owner.Creature, 1M, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 1, false));
        InvokeEnergyCostChanged();
    }
}

public class SpecSpecial01() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override bool ShouldGlowGoldInternal =>
        LoudCrowCardLogic.IsMagicBulletPrimed(this);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.MagicBulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10M, ValueProp.Move),
        new DynamicVar("DamagePerConsumedBullet", 2M),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        var opponents = combatState
            .GetOpponentsOf(Owner.Creature)
            .Where(creature => creature.IsAlive)
            .ToList();

        if (!LoudCrowCardLogic.HasMagicBullet(Owner))
        {
            foreach (var opponent in opponents)
                await CreatureCmd.Damage(choiceContext, opponent, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this);
            return;
        }

        await LoudCrowCardLogic.ConsumeMagicBullet(Owner, choiceContext, this);
        var consumedBullets = await BulletSystem.ConsumeBulletsForAttackWithEffects(choiceContext, Owner.Creature, 3);
        decimal combinedBaseDamage =
            DynamicVars.Damage.BaseValue +
            (DynamicVars["DamagePerConsumedBullet"].BaseValue * consumedBullets.Count);

        foreach (var opponent in opponents)
        {
            decimal totalDamage = BulletSystem.GetConsumedBulletModifiedDamage(
                combinedBaseDamage,
                opponent,
                consumedBullets,
                Owner.Creature);
            await CreatureCmd.Damage(choiceContext, opponent, totalDamage, ValueProp.Move, Owner.Creature, this);
            if (consumedBullets.Count > 0)
                await BulletSystem.ApplyConsumedBulletPostAttackEffects(Owner.Creature, this, opponent, consumedBullets);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1M);
        DynamicVars["DamagePerConsumedBullet"].UpgradeValueBy(1M);
    }
}

public class SpecSpecial03() : LoudCrowCardModel(
    0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override bool HasEnergyCostX => true;

    protected override bool ShouldGlowGoldInternal =>
        LoudCrowCardLogic.IsMagicBulletPrimed(this);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.MagicBulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(6M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int hitCount = ResolveEnergyXValue();
        if (LoudCrowCardLogic.HasMagicBullet(Owner))
        {
            hitCount += 2;
            await LoudCrowCardLogic.ConsumeMagicBullet(Owner, choiceContext, this);
        }

        if (hitCount <= 0)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).WithHitCount(hitCount).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3M);
}

public class SpecCommon01() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(13M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await BulletSystem.SpendBullets(choiceContext, Owner.Creature, 2);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4M);
}

public class FocusedFire() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var consumedBullets = await BulletSystem.ConsumeBulletsForAttackWithEffects(choiceContext, Owner.Creature, 3);
        if (play.Target != null)
        {
            decimal damage = BulletSystem.GetConsumedBulletModifiedDamage(
                DynamicVars.Damage.BaseValue,
                play.Target,
                consumedBullets,
                Owner.Creature);
            await DamageCmd.Attack(damage)
                .FromCard(this).Targeting(play.Target).Execute(choiceContext);
            await BulletSystem.ApplyConsumedBulletPostAttackEffects(Owner.Creature, this, play.Target, consumedBullets);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3M);
}

public class SpecRare04() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletAndMagicBullet;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<LoadedMagicBulletPower>(Owner.Creature, 1M, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, false));
        InvokeEnergyCostChanged();
    }
}

public class SpecRare06() : LoudCrowCardModel(
    0, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("InitialBullets", 6M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await LoudCrowCardLogic.QueueRandomBullets(Owner, this, (int)DynamicVars["InitialBullets"].BaseValue);
        await PowerCmd.Apply<SpecRare06Power>(Owner.Creature, 1M, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["InitialBullets"].UpgradeValueBy(3M);
}

public class SpecRare08() : LoudCrowCardModel(
    0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override bool IsPlayable =>
        Owner?.Creature == null || LoudCrowCardLogic.BulletCount(Owner) >= 5;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(10M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var consumedBullets = await BulletSystem.ConsumeBulletsForAttackWithEffects(choiceContext, Owner.Creature, 5);
        if (play.Target != null)
        {
            decimal damage = BulletSystem.GetConsumedBulletModifiedDamage(
                DynamicVars.Damage.BaseValue,
                play.Target,
                consumedBullets,
                Owner.Creature);
            await DamageCmd.Attack(damage)
                .FromCard(this).Targeting(play.Target).Execute(choiceContext);
            await BulletSystem.ApplyConsumedBulletPostAttackEffects(Owner.Creature, this, play.Target, consumedBullets);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4M);
}

public class BombardmentMagicBullet() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2M);
}

public class Malice() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(10M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var target = play.Target!;
        if (target.GetPower(ModelDb.GetId(typeof(ArtifactPower))) is ArtifactPower)
            await PowerCmd.Remove<ArtifactPower>(target);

        decimal damage = DynamicVars.Damage.BaseValue;
        if (target.GetPower(ModelDb.GetId(typeof(MinionPower))) is MinionPower)
            damage *= 2M;

        await DamageCmd.Attack(damage)
            .FromCard(this).Targeting(target).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3M);
}

public class SpecRare02() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
        if (!IsDupe && !LoudCrowTurnEffects.ShouldSuppressSpecRare02Activation(this))
        {
            LoudCrowTurnEffects.ActivateSpecRare02(Owner.Creature);
            await PowerCmd.Apply<SpecRare02Power>(
                Owner.Creature,
                1M,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2M);
}

public class VolleyFire() : LoudCrowCardModel(
    2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(10M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);

        var hand = Owner.Piles.FirstOrDefault(pile => pile.Type == PileType.Hand);
        var otherShots = hand?.Cards
            .Where(card => card != this && LoudCrowCardLogic.IsShotCard(card))
            .ToList() ?? [];

        foreach (var shot in otherShots)
        {
            var target = shot.TargetType == TargetType.AnyEnemy ? play.Target : null;
            await CardCmd.AutoPlay(choiceContext, shot, target);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4M);
}

public class SpecSpecial09() : LoudCrowCardModel(
    0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int hitCount = LoudCrowTurnEffects.HasNegativeEffect(Owner.Creature) ? 2 : 1;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).WithHitCount(hitCount).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2M);
}

public class SpecSpecial06 : LoudCrowCardModel
{
    public SpecSpecial06()
        : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(4M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);

        BaseReplayCount += 1;
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2M);
}

public class NoGuard() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal block = LoudCrowCardLogic.GetCreatureBlock(Owner.Creature) ?? 0M;
        LoudCrowCardLogic.SetCreatureBlock(Owner.Creature, 0M);
        if (block <= 0)
            return;

        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        var opponents = combatState
            .GetOpponentsOf(Owner.Creature)
            .Where(creature => creature.IsAlive)
            .ToList();

        foreach (var opponent in opponents)
            await CreatureCmd.Damage(choiceContext, opponent, block, ValueProp.Move, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 0, false));
        InvokeEnergyCostChanged();
    }
}

public class SpecRare03() : LoudCrowCardModel(
    2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override bool ShouldGlowGoldInternal =>
        LoudCrowCardLogic.IsMagicBulletPrimed(this);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.MagicBulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10M, ValueProp.Move),
        new DynamicVar("DamagePerExhaustedCard", 3M),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = Owner.Piles.FirstOrDefault(pile => pile.Type == PileType.Hand);
        var cardsToExhaust = hand?.Cards.Where(card => card != this).ToList() ?? [];
        int exhaustedCount = 0;
        foreach (var card in cardsToExhaust)
        {
            exhaustedCount++;
            await CardCmd.Exhaust(choiceContext, card);
        }

        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        decimal totalDamage = DynamicVars.Damage.BaseValue;
        bool magicBullet = exhaustedCount > 0 && LoudCrowCardLogic.HasMagicBullet(Owner);
        if (magicBullet)
        {
            await LoudCrowCardLogic.ConsumeMagicBullet(Owner, choiceContext, this);
            totalDamage += DynamicVars["DamagePerExhaustedCard"].BaseValue * exhaustedCount;
        }

        await DamageCmd.Attack(totalDamage)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5M);
        DynamicVars["DamagePerExhaustedCard"].UpgradeValueBy(1M);
    }
}

public class PerfectFinish() : LoudCrowCardModel(
    1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(10M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<PerfectFinishPower>(Owner.Creature, DynamicVars.Block.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4M);
}

public class SelfImmolation() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.VulnerableWeakAndFrailOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("EnergyGain", 2M),
        new DynamicVar("DrawCount", 2M),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var debuffs = new List<Func<Task>>
        {
            () => PowerCmd.Apply<VulnerablePower>(Owner.Creature, 1M, Owner.Creature, this),
            () => PowerCmd.Apply<WeakPower>(Owner.Creature, 1M, Owner.Creature, this),
            () => PowerCmd.Apply<FrailPower>(Owner.Creature, 1M, Owner.Creature, this),
        };

        for (int i = debuffs.Count - 1; i > 0; i--)
        {
            int swapIndex = Owner.RunState.Rng.CombatCardSelection.NextInt(i + 1);
            (debuffs[i], debuffs[swapIndex]) = (debuffs[swapIndex], debuffs[i]);
        }

        await debuffs[0]();
        await debuffs[1]();

        Owner.PlayerCombatState?.GainEnergy(DynamicVars["EnergyGain"].BaseValue);
        await CardPileCmd.Draw(choiceContext, DynamicVars["DrawCount"].BaseValue, Owner, false);
    }

    protected override void OnUpgrade() => DynamicVars["EnergyGain"].UpgradeValueBy(1M);
}

public class SpecRare09() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("ReturnCount", 2M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var cards = LoudCrowCardLogic.TakeRandomExhaustCards(Owner, (int)DynamicVars["ReturnCount"].BaseValue);
        foreach (var card in cards)
            await CardPileCmd.Add(card, PileType.Hand, source: this);
    }

    protected override void OnUpgrade() => DynamicVars["ReturnCount"].UpgradeValueBy(1M);
}

public class SpecRare10() : LoudCrowCardModel(
    1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<SpecRare10Power>(Owner.Creature, 1M, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}

[Pool(typeof(TokenCardPool))]
public class Release() : LoudCrowCardModel(
    0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, false)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3M, ValueProp.Move),
        new DynamicVar("DrawCount", 1M),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
        await CardPileCmd.Draw(choiceContext, DynamicVars["DrawCount"].BaseValue, Owner, false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2M);
    }
}

public class Duck() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal =>
        LoudCrowCardLogic.IsMagicBulletPrimed(this);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.MagicBulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4M, ValueProp.Move),
        new BlockVar("MagicBulletBlock", 4M, ValueProp.Move),
    ];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal block = DynamicVars.Block.BaseValue;
        if (LoudCrowCardLogic.HasMagicBullet(Owner))
        {
            block += DynamicVars["MagicBulletBlock"].BaseValue;
            await LoudCrowCardLogic.ConsumeMagicBullet(Owner, choiceContext, this);
        }

        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1M);
        DynamicVars["MagicBulletBlock"].UpgradeValueBy(1M);
    }
}
// ???? ??占쎈닔??????????????????????????????????????????????????????????????????????????????????????????????????
public class OverheatShot() : LoudCrowCardModel(
    0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowCardLogic.GetHeatHoverTips();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new ShotDamageVar(8M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
        await LoudCrowCardLogic.AddHeatToDiscard(Owner, this, 1);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2M);
}
// ???? ???占쏀뀬 ????????????????????????????????????????????????????????????????????????????????????????????
public class RapidFire() : LoudCrowCardModel(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal =>
        LoudCrowCardLogic.IsMagicBulletPrimed(this);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.MagicBulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(3M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        bool magicBullet = LoudCrowCardLogic.HasMagicBullet(Owner);
        int hitCount = magicBullet ? 3 : 2;
        if (magicBullet)
            await LoudCrowCardLogic.ConsumeMagicBullet(Owner, choiceContext, this);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).WithHitCount(hitCount).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1M);
}
// ???? 占?占쏙옙?????濡ャ걗??占쎈뼬??????????????????????????????????????????????????????????????????????????????????????????????
public class PistolWhip() : LoudCrowCardModel(
    0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override bool IsPlayable =>
        Owner?.Creature == null || !BulletSystem.HasAnyBullet(Owner.Creature);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(13M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4M);
}
// ???? 繞벿븐뫓占??????????????????????????????????????????????????????????????????????????????????????????????
public class Scram() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(8M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await BulletSystem.SpendBullets(choiceContext, Owner.Creature, 1);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3M);
}
// ???? 濚밸Ŧ?占썲퐲琉몃돥? 濚밸Þ?占썼땻?????????????????????????????????????????????????????????????????????????????????????????????
public class CrowFeather() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.StrengthOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("StrengthGain", 3M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        int strengthAmount = (int)DynamicVars["StrengthGain"].BaseValue;
        if (strengthAmount <= 0)
            return;

        await LoudCrowRelicEffects.ApplyVisibleTemporaryStrength(Owner.Creature, strengthAmount, this);
    }

    protected override void OnUpgrade() => DynamicVars["StrengthGain"].UpgradeValueBy(2M);
}
// ???? ??瑜곷턄??????????????????????????????????????????????????????????????????????????????????????????????
public class HighNoon() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.VulnerableOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Vulnerable", 2M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        foreach (var enemy in combatState.GetOpponentsOf(Owner.Creature).Where(c => c.IsAlive))
            await PowerCmd.Apply<VulnerablePower>(enemy, DynamicVars["Vulnerable"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Vulnerable"].UpgradeValueBy(1M);
}
// ???? ???占쎈룴 ??????????????????????????????????????????????????????????????????????????????????????????????
public class SpareAmmo() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BulletCount", 3M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await LoudCrowCardLogic.QueueRandomBullets(Owner, this, (int)DynamicVars["BulletCount"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["BulletCount"].UpgradeValueBy(2M);
}
// ???? ???占썩뵛 ????????????????????????????????????????????????????????????????????????????????????????????
public class MostWanted() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BulletGain", 1M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target != null)
            await PowerCmd.Apply<MostWantedPower>(play.Target, 1M, Owner.Creature, this);

        await LoudCrowCardLogic.QueueRandomBullets(
            Owner,
            this,
            (int)DynamicVars["BulletGain"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BulletGain"].UpgradeValueBy(2M);
    }
}

public class FullBurst() : LoudCrowCardModel(
    0, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies)
{
    protected override bool ShouldGlowGoldInternal =>
        LoudCrowCardLogic.IsMagicBulletPrimed(this);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.MagicBulletOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9M, ValueProp.Move),
        new DynamicVar("MagicBulletDamage", 12M),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = Owner.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand != null && hand.Cards.Any())
        {
            var selectedCards = await CardSelectCmd.FromHandForDiscard(
                choiceContext,
                Owner,
                new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1),
                null,
                this);

            foreach (var selected in selectedCards)
                await CardCmd.Discard(choiceContext, selected);
        }

        var combatState = Owner.Creature.CombatState;
        if (combatState == null)
            return;

        bool magicBullet = LoudCrowCardLogic.HasMagicBullet(Owner);
        decimal damage = magicBullet
            ? DynamicVars["MagicBulletDamage"].BaseValue
            : DynamicVars.Damage.BaseValue;

        if (magicBullet)
            await LoudCrowCardLogic.ConsumeMagicBullet(Owner, choiceContext, this);

        decimal blockGained = combatState
            .GetOpponentsOf(Owner.Creature)
            .Where(creature => creature.IsAlive)
            .Sum(creature => LoudCrowCardLogic.EstimateUnblockedDamage(this, creature, damage));

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .TargetingAllOpponents(combatState)
            .Execute(choiceContext);

        if (blockGained > 0M)
            await CreatureCmd.GainBlock(Owner.Creature, blockGained, ValueProp.Move, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3M);
        DynamicVars["MagicBulletDamage"].UpgradeValueBy(3M);
    }
}

public class MakeshiftMeasure() : LoudCrowCardModel(
    1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(13M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
        await PowerCmd.Apply<MakeshiftMeasureFrailPower>(Owner.Creature, 1M, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4M);
}

public class AmmoSupply() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BulletCount", 2M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        int repeatCount = ResolveEnergyXValue();
        if (repeatCount <= 0)
            return;

        await LoudCrowCardLogic.QueueRandomBullets(
            Owner,
            this,
            repeatCount * (int)DynamicVars["BulletCount"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["BulletCount"].UpgradeValueBy(1M);
}

public class Reloading() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        LoudCrowKeywordHoverTips.BulletOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("BulletCount", 1M)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = Owner.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        int discardCount = hand == null ? 0 : System.Math.Min(2, hand.Cards.Count);
        if (discardCount > 0)
        {
            var selectedCards = await CardSelectCmd.FromHandForDiscard(
                choiceContext,
                Owner,
                new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, discardCount),
                null,
                this);

            foreach (var selected in selectedCards)
                await CardCmd.Discard(choiceContext, selected);
        }

        await LoudCrowCardLogic.QueueRandomBullets(Owner, this, (int)DynamicVars["BulletCount"].BaseValue);
        await CardPileCmd.Draw(choiceContext, 3M, Owner, false);
    }

    protected override void OnUpgrade() => DynamicVars["BulletCount"].UpgradeValueBy(1M);
}

[Pool(typeof(StatusCardPool))]
public class Heat() : LoudCrowCardModel(
    0, CardType.Status, CardRarity.Status, TargetType.Self, showInCardLibrary: true)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Ethereal, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(2M)
            .FromCard(this).Targeting(Owner.Creature).Unpowered().Execute(choiceContext);
    }

    protected override void OnUpgrade() { }
}

[Pool(typeof(TokenCardPool))]
public class PlankPiece() : LoudCrowCardModel(
    0, CardType.Skill, CardRarity.Token, TargetType.Self, showInCardLibrary: false)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Retain, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(2M, ValueProp.Move)];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1M);
    }
}

public class Outlaw() : LoudCrowCardModel(
    3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async System.Threading.Tasks.Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<OutlawPower>(Owner.Creature, 1M, Owner.Creature, this);
        LoudCrowTurnEffects.ActivateOutlaw(Owner.Creature);
        LoudCrowCardLogic.ApplyOutlawToHand(Owner);
    }

    protected override void OnUpgrade()
    {
        MockSetEnergyCost(new CardEnergyCost(this, 2, false));
        InvokeEnergyCostChanged();
    }
}

internal static class LoudCrowKeywordHoverTips
{
    public static readonly IHoverTip MagicBullet = new HoverTip(
        new LocString("cards", "LOUDCROWMOD-MAGIC_BULLET.title"),
        new LocString("cards", "LOUDCROWMOD-MAGIC_BULLET.description"),
        null!);

    public static readonly IHoverTip Bullet = new HoverTip(
        new LocString("cards", "LOUDCROWMOD-BULLET.title"),
        new LocString("cards", "LOUDCROWMOD-BULLET.description"),
        null!);

    public static readonly IHoverTip Vulnerable = HoverTipFactory.FromPower<VulnerablePower>();
    public static readonly IHoverTip Weak = HoverTipFactory.FromPower<WeakPower>();
    public static readonly IHoverTip Frail = HoverTipFactory.FromPower<FrailPower>();
    public static readonly IHoverTip Strength = HoverTipFactory.FromPower<StrengthPower>();

    public static readonly IHoverTip HighCaliberBullet = new HoverTip(
        new LocString("cards", "LOUDCROWMOD-HIGH_CALIBER_ROUND.title"),
        new LocString("cards", "LOUDCROWMOD-HIGH_CALIBER_ROUND.description"),
        null!);

    public static readonly IHoverTip HeatCard = HoverTipFactory.FromCard<Heat>();
    public static readonly IHoverTip ReleaseCard = HoverTipFactory.FromCard<Release>();
    public static readonly IHoverTip ReleaseCardUpgraded = HoverTipFactory.FromCard<Release>(upgrade: true);
    public static readonly IHoverTip PlankPieceCard = HoverTipFactory.FromCard<PlankPiece>();
    public static readonly IHoverTip PlankPieceCardUpgraded = HoverTipFactory.FromCard<PlankPiece>(upgrade: true);

    public static readonly IReadOnlyList<IHoverTip> MagicBulletOnly = [MagicBullet];
    public static readonly IReadOnlyList<IHoverTip> BulletOnly = [Bullet];
    public static readonly IReadOnlyList<IHoverTip> VulnerableOnly = [Vulnerable];
    public static readonly IReadOnlyList<IHoverTip> WeakOnly = [Weak];
    public static readonly IReadOnlyList<IHoverTip> StrengthOnly = [Strength];
    public static readonly IReadOnlyList<IHoverTip> VulnerableAndWeakOnly = [Vulnerable, Weak];
    public static readonly IReadOnlyList<IHoverTip> VulnerableWeakAndFrailOnly = [Vulnerable, Weak, Frail];
    public static readonly IReadOnlyList<IHoverTip> WeakAndStrengthOnly = [Weak, Strength];
    public static readonly IReadOnlyList<IHoverTip> VulnerableWeakAndStrengthOnly = [Vulnerable, Weak, Strength];
    public static readonly IReadOnlyList<IHoverTip> BulletAndStrengthOnly = [Bullet, Strength];
    public static readonly IReadOnlyList<IHoverTip> HighCaliberBulletOnly = [HighCaliberBullet];
    public static readonly IReadOnlyList<IHoverTip> BulletAndHighCaliberBullet = [Bullet, HighCaliberBullet];
    public static readonly IReadOnlyList<IHoverTip> BulletAndMagicBullet = [Bullet, MagicBullet];
    public static readonly IReadOnlyList<IHoverTip> HeatCardOnly = [HeatCard];
    public static readonly IReadOnlyList<IHoverTip> ReleaseCardOnly = [ReleaseCard];
    public static readonly IReadOnlyList<IHoverTip> ReleaseCardUpgradedOnly = [ReleaseCardUpgraded];
    public static readonly IReadOnlyList<IHoverTip> PlankPieceOnly = [PlankPieceCard];
    public static readonly IReadOnlyList<IHoverTip> PlankPieceUpgradedOnly = [PlankPieceCardUpgraded];
    public static readonly IReadOnlyList<IHoverTip> HeatAndReleaseOnly = [HeatCard, ReleaseCard];
    public static readonly IReadOnlyList<IHoverTip> HeatAndReleaseUpgradedOnly = [HeatCard, ReleaseCardUpgraded];
}







