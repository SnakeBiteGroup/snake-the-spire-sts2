using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(HiddenDaggers))]
public static class HiddenDaggersPatch
{
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(HiddenDaggers __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = new List<IHoverTip> { HoverTipFactory.FromCard<Snakebite>(__instance.IsUpgraded) };
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(HiddenDaggers __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(HiddenDaggers instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardCmd.Discard(choiceContext, await CardSelectCmd.FromHandForDiscard(choiceContext, instance.Owner, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, instance.DynamicVars.Cards.IntValue), null, instance));
        
        List<CardModel> cardsToAdd = new List<CardModel>();
        int snakebiteCount = instance.DynamicVars["Shivs"].IntValue;
        for (int i = 0; i < snakebiteCount; i++)
        {
            cardsToAdd.Add(instance.CombatState.CreateCard<Snakebite>(instance.Owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(cardsToAdd, PileType.Hand, addedByPlayer: true);
        
        if (instance.IsUpgraded)
        {
            foreach (CardModel item in cardsToAdd)
            {
                CardCmd.Upgrade(item);
            }
        }
    }
}
