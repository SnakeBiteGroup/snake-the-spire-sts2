using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace SBMod.SBModCode.CardsFix.Silent;

[HarmonyPatch(typeof(KnifeTrap))]
public static class KnifeTrapPatch
{
    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(KnifeTrap __instance, ref IEnumerable<DynamicVar> __result)
    {
        var list = __result?.ToList() ?? new List<DynamicVar>();
        if (!list.Any(v => v.Name == "CalculatedBites"))
        {
            list.Add(new CalculatedVar("CalculatedBites").WithMultiplier((CardModel card, Creature? _) => PileType.Exhaust.GetPile(card.Owner).Cards.Count((CardModel c) => c is Snakebite) 
                + PileType.Draw.GetPile(card.Owner).Cards.Count((CardModel c) => c is Snakebite) + PileType.Discard.GetPile(card.Owner).Cards.Count((CardModel c) => c is Snakebite)));
        }
        __result = list;
    }

    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(KnifeTrap __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = new List<IHoverTip> { HoverTipFactory.FromCard<Shiv>(__instance.IsUpgraded), HoverTipFactory.FromCard<Snakebite>() };
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(KnifeTrap __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(KnifeTrap instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        
        IEnumerable<CardModel> shivCards = PileType.Exhaust.GetPile(instance.Owner).Cards.Where((CardModel c) => c.Tags.Contains(CardTag.Shiv)).ToList();
        var snakebiteDrawCards = PileType.Draw.GetPile(instance.Owner).Cards.Where(c => c is Snakebite).ToList();
        var snakebiteDiscardCards = PileType.Discard.GetPile(instance.Owner).Cards.Where(c => c is Snakebite).ToList();
        var allSnakebites = snakebiteDrawCards.Concat(snakebiteDiscardCards).ToList();
        
        bool firstPlay = true;
        
        foreach (var card in shivCards)
        {
            if (instance.IsUpgraded)
            {
                CardCmd.Upgrade(card, CardPreviewStyle.None);
            }
            await CardCmd.AutoPlay(choiceContext, card, cardPlay.Target, AutoPlayType.Default, skipXCapture: false, !firstPlay);
            firstPlay = false;
        }
        
        foreach (var card in allSnakebites)
        {
            if (instance.IsUpgraded)
            {
                CardCmd.Upgrade(card, CardPreviewStyle.None);
            }
            await CardCmd.AutoPlay(choiceContext, card, cardPlay.Target, AutoPlayType.Default, skipXCapture: false, !firstPlay);
            firstPlay = false;
        }
    }
}
