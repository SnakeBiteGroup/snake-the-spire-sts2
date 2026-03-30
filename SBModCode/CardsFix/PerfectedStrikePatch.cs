using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.ValueProps;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(PerfectedStrike))]
public static class PerfectedStrikePatch
{
    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(PerfectedStrike __instance, ref IEnumerable<DynamicVar> __result)
    {
        var vars = __result.ToList();
        for (int i = 0; i < vars.Count; i++)
        {
            if (vars[i] is CalculatedDamageVar calcVar)
            {
                vars[i] = new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => 
                    card.Owner.PlayerCombatState.AllCards.Count((CardModel card) => card is Snakebite));
            }
        }
        __result = vars;
    }

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
