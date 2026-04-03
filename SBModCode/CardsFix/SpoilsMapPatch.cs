using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Reflection.Emit;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.TestSupport;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(SpoilsMap))]
public static class SpoilsMapPatch
{
    [HarmonyPatch("MaxUpgradeLevel", MethodType.Getter)]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> MaxUpgradeLevelTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new List<CodeInstruction>
        {
            new CodeInstruction(OpCodes.Ldc_I4, int.MaxValue),
            new CodeInstruction(OpCodes.Ret)
        }.AsEnumerable();
    }

    [HarmonyPatch("OnQuestComplete")]
    [HarmonyPrefix]
    static bool OnQuestCompletePrefix(SpoilsMap __instance, ref Task<int> __result)
    {
        int snakeCount = (int)__instance.DynamicVars.Gold.IntValue;
        
        List<CardModel> cardsToAdd = new List<CardModel>();
        for (int i = 0; i < snakeCount; i++)
        {
            cardsToAdd.Add(__instance.Owner.RunState.CreateCard<Snakebite>(__instance.Owner));
        }
        
        var results = CardPileCmd.Add(cardsToAdd, PileType.Deck).Result;
        
        if (!TestMode.IsOn && 
            !CombatManager.Instance.IsEnding && 
            LocalContext.IsMine(__instance))
        {
            CardCmd.PreviewCardPileAdd(results, time: 3f, CardPreviewStyle.MessyLayout);
        }
        
        PlayerCmd.CompleteQuest(__instance);
        CardPileCmd.RemoveFromDeck(__instance).Wait();
        
        __result = Task.FromResult(snakeCount);
        return false;
    }
}
