using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.CardsFix.Defect;

[HarmonyPatch(typeof(DoubleEnergy))]
public static class DoubleEnergyPatch
{
    
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(CrashLanding __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = HoverTipFactory.FromCardWithCardHoverTips<Snakebite>();
    }
    
    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(DoubleEnergy __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(DoubleEnergy instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> cardsToDuplicate = new List<CardModel>();

        var handCards = CardPile.GetCards(instance.Owner, PileType.Hand);
        foreach (var card in handCards)
        {
            if (card is Snakebite)
            {
                cardsToDuplicate.Add(card);
            }
        }

        var discardCards = CardPile.GetCards(instance.Owner, PileType.Discard);
        foreach (var card in discardCards)
        {
            if (card is Snakebite)
            {
                cardsToDuplicate.Add(card);
            }
        }

        var exhaustCards = CardPile.GetCards(instance.Owner, PileType.Exhaust);
        foreach (var card in exhaustCards)
        {
            if (card is Snakebite)
            {
                cardsToDuplicate.Add(card);
            }
        }

        var drawCards = CardPile.GetCards(instance.Owner, PileType.Draw);
        foreach (var card in drawCards)
        {
            if (card is Snakebite)
            {
                cardsToDuplicate.Add(card);
            }
        }

        foreach (var originalCard in cardsToDuplicate)
        {
            var clone = originalCard.CreateClone();
            await CardPileCmd.Add(clone, originalCard.Pile.Type);
        }
    }
}
