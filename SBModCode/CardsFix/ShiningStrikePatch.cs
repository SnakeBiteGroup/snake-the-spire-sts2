using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(ShiningStrike))]
public static class ShiningStrikePatch
{
    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(ShiningStrike __instance, ref IEnumerable<DynamicVar> __result)
    {
        var list = __result?.ToList() ?? new List<DynamicVar>();
            list.Add(new PowerVar<PoisonPower>(7m));
        __result = list;
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(ShiningStrike __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(ShiningStrike instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var enemy in instance.CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<PoisonPower>(enemy, instance.DynamicVars.Poison.BaseValue, instance.Owner.Creature, instance);
        }
        await PlayerCmd.GainStars(instance.DynamicVars.Stars.BaseValue, instance.Owner);
        if (!instance.Keywords.Contains(CardKeyword.Exhaust) && !instance.ExhaustOnNextPlay)
        {
            await CardPileCmd.Add(instance, PileType.Draw, CardPilePosition.Top);
        }
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(ShiningStrike __instance)
    {
        __instance.DynamicVars.Poison.UpgradeValueBy(3m);
        return false;
    }
}
