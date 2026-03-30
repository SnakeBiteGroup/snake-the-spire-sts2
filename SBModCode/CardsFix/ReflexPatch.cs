using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(Reflex))]
public static class ReflexPatch
{

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(Reflex __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(Reflex instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var discardedCard = (await CardSelectCmd.FromHandForDiscard(choiceContext, instance.Owner, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1), null, instance)).FirstOrDefault();
        
        if (discardedCard != null)
        {
            await CardCmd.Discard(choiceContext, discardedCard);
            
            bool isSnakebite = discardedCard is Snakebite || discardedCard.Id.Entry.ToLowerInvariant().Contains("snakebite");
            
            if (isSnakebite)
            {
                await CardPileCmd.Draw(choiceContext, instance.DynamicVars.Cards.BaseValue, instance.Owner);
            }
        }
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(Reflex __instance)
    {
        __instance.DynamicVars.Cards.UpgradeValueBy(1m);
        return false;
    }
}
