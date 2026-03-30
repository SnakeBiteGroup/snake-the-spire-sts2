using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.Powers;

public sealed class SnakebiteGuidePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
    {
        new CardsVar(2)
    };

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != Owner.Side || AmountOnTurnStart == 0)
        {
            return;
        }

        var cardsToMove = new List<CardModel>();
        int remaining = Amount;

        var drawPile = PileType.Draw.GetPile(Owner.Player);
        foreach (var card in drawPile.Cards.ToList())
        {
            if (remaining <= 0) break;
            if (card is Snakebite)
            {
                cardsToMove.Add(card);
                remaining--;
            }
        }

        if (remaining > 0)
        {
            var discardPile = PileType.Discard.GetPile(Owner.Player);
            foreach (var card in discardPile.Cards.ToList())
            {
                if (remaining <= 0) break;
                if (card is Snakebite)
                {
                    cardsToMove.Add(card);
                    remaining--;
                }
            }
        }

        if (cardsToMove.Count > 0)
        {
            foreach (var card in cardsToMove)
            {
                await CardPileCmd.Add(card, PileType.Hand);
            }
        }

        if (remaining > 0)
        {
            ThinkCmd.Play(new LocString("combat_messages", "NO_DRAW_SNAKEBITES"), Owner, 2.0);
        }

        await PowerCmd.Remove(this);
    }
}
