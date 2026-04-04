using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.CardsFix.Silent;

[HarmonyPatch(typeof(GrandFinale))]
public static class GrandFinalePatch
{
    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(GrandFinale __instance, ref IEnumerable<DynamicVar> __result)
    {
        __result = new List<DynamicVar>
        {
            new PowerVar<PoisonPower>(60m)
        };
    }

    [HarmonyPatch("IsPlayable", MethodType.Getter)]
    [HarmonyPostfix]
    static void IsPlayablePostfix(GrandFinale __instance, ref bool __result)
    {
        var drawPile = PileType.Draw.GetPile(__instance.Owner);
        __result = drawPile.Cards.All(card => card is Snakebite);
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(GrandFinale __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(GrandFinale instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(instance.Owner.Creature, "Cast", instance.Owner.Character.CastAnimDelay);
        var hittableEnemies = instance.CombatState.HittableEnemies;
        foreach (var enemy in hittableEnemies)
        {
            VfxCmd.PlayOnCreatureCenter(enemy, "vfx/vfx_bite");
            await PowerCmd.Apply<PoisonPower>(enemy, instance.DynamicVars.Poison.BaseValue, instance.Owner.Creature, instance);
        }
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(GrandFinale __instance)
    {
        __instance.DynamicVars.Poison.UpgradeValueBy(15m);
        return false;
    }
}
