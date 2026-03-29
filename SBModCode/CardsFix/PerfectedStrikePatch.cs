using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(PerfectedStrike))]
public static class PerfectedStrikePatch
{
    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(PerfectedStrike __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(PerfectedStrike instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var snakebiteCount = instance.Owner.PlayerCombatState.AllCards.Count((CardModel c) => c is Snakebite);
        int poisonAmount = (int)instance.DynamicVars.CalculatedDamage.BaseValue + (snakebiteCount * (int)instance.DynamicVars.ExtraDamage.BaseValue);
        await PowerCmd.Apply<PoisonPower>(cardPlay.Target, poisonAmount, instance.Owner.Creature, instance);
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(PerfectedStrike __instance)
    {
        __instance.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
        return false;
    }
}
