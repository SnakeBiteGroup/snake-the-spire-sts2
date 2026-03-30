using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SBMod.SBModCode.PowersFix;

[HarmonyPatch(typeof(RollingBoulderPower))]
public static class RollingBoulderPowerPatch
{
    [HarmonyPatch("DoDamage")]
    [HarmonyPrefix]
    static bool DoDamagePrefix(RollingBoulderPower __instance, PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, ref Task<IEnumerable<DamageResult>> __result)
    {
        __result = PatchDoDamage(__instance, choiceContext, targets);
        return false;
    }

    static async Task<IEnumerable<DamageResult>> PatchDoDamage(RollingBoulderPower instance, PlayerChoiceContext choiceContext, IEnumerable<Creature> targets)
    {
        List<DamageResult> results = new List<DamageResult>();
        foreach (var target in targets)
        {
            await PowerCmd.Apply<PoisonPower>(target, instance.Amount, instance.Owner, null);
            results.Add(new DamageResult(target, ValueProp.Unpowered));
        }
        return results;
    }
}
