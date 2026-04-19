using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.CardsFix.Curse;

[HarmonyPatch(typeof(Normality))]
public static class NormalityPatch
{
    [HarmonyPatch("ShouldPreventCardPlay", MethodType.Getter)]
    [HarmonyPostfix]
    static void ShouldPreventCardPlayPostfix(Normality __instance, ref bool __result)
    {
        int cardsPlayedThisTurn = CombatManager.Instance.History.CardPlaysStarted.Count((CardPlayStartedEntry e) => 
            e.HappenedThisTurn(__instance.CombatState) && e.CardPlay.Card.Owner == __instance.Owner);
        
        __result = cardsPlayedThisTurn >= 3;
    }

    [HarmonyPatch("ShouldPlay")]
    [HarmonyPrefix]
    static bool ShouldPlayPrefix(Normality __instance, CardModel card, AutoPlayType _, ref bool __result)
    {
        if (card.Owner != __instance.Owner)
        {
            __result = true;
            return false;
        }
        
        CardPile? pile = __instance.Pile;
        if (pile == null || pile.Type != PileType.Hand)
        {
            __result = true;
            return false;
        }
        
        int cardsPlayedThisTurn = CombatManager.Instance.History.CardPlaysStarted.Count((CardPlayStartedEntry e) => 
            e.HappenedThisTurn(__instance.CombatState) && e.CardPlay.Card.Owner == __instance.Owner);
        
        bool shouldPrevent = cardsPlayedThisTurn >= 3;
        
        if (shouldPrevent)
        {
            __result = false;
            return false;
        }
        
        bool isSnakebite = card is Snakebite;
        __result = isSnakebite;
        return false;
    }
}
