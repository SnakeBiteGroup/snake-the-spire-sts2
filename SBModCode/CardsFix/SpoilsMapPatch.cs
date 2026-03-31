using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(SpoilsMap))]
public static class SpoilsMapPatch
{
    [HarmonyPatch("OnQuestComplete")]
    [HarmonyPrefix]
    static bool OnQuestCompletePrefix(SpoilsMap __instance, ref Task<int> __result)
    {
        __result = PatchOnQuestComplete(__instance);
        return false;
    }

    static async Task<int> PatchOnQuestComplete(SpoilsMap instance)
    {
        List<CardModel> cardsToAdd = new List<CardModel>();
        for (int i = 0; i < 600; i++)
        {
            cardsToAdd.Add(instance.Owner.RunState.CreateCard<Snakebite>(instance.Owner));
        }
        await CardPileCmd.Add(cardsToAdd, PileType.Deck);
        PlayerCmd.CompleteQuest(instance);
        await CardPileCmd.RemoveFromDeck(instance);
        return 600;
    }
}