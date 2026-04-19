using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace SBMod.SBModCode.Enchantments;

public sealed class SerpentStrikeEnchant : EnchantmentModel
{
    public override bool ShowAmount => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            var tips = new List<IHoverTip>();
            int replayCount = Card?.GetEnchantedReplayCount() ?? 0;
            if (replayCount > 0)
            {
                tips.Add(HoverTipFactory.Static(StaticHoverTip.ReplayDynamic, new DynamicVar("Times", replayCount)));
            }
            return tips;
        }
    }

    public override bool CanEnchant(CardModel card)
    {
        if (base.CanEnchant(card) && !card.Keywords.Contains(CardKeyword.Unplayable))
        {
            return !card.EnergyCost.CostsX;
        }
        return false;
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != Card)
        {
            return Task.CompletedTask;
        }
        if (Card.Pile.Type != PileType.Hand)
        {
            return Task.CompletedTask;
        }

        int energyCost = Card.Owner.RunState.Rng.CombatEnergyCosts.NextInt(4);
        Card.EnergyCost.SetThisCombat(energyCost);
        Card.BaseReplayCount = energyCost;
        NCard.FindOnTable(card)?.PlayRandomizeCostAnim();
        return Task.CompletedTask;
    }

    public override int EnchantPlayCount(int originalPlayCount)
    {
        return Card?.BaseReplayCount ?? originalPlayCount;
    }
}
