using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SBMod.SBModCode.CardsFix.Silent;

[HarmonyPatch(typeof(StrikeSilent))]
public static class StrikeSilentPatch
{
    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(StrikeSilent __instance, ref IEnumerable<DynamicVar> __result)
    {
        var list = __result?.ToList() ?? new List<DynamicVar>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] is DamageVar)
            {
                list[i] = new DamageVar(4m, ValueProp.Move);
            }
        }
        if (!list.Any(v => v.Name == "PoisonPower"))
        {
            list.Add(new PowerVar<PoisonPower>(2m));
        }
        __result = list;
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(StrikeSilent __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(StrikeSilent instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(instance.DynamicVars.Damage.BaseValue).FromCard(instance).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        VfxCmd.PlayOnCreatureCenter(cardPlay.Target, "vfx/vfx_bite");
        await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, instance.DynamicVars.Poison.BaseValue, instance.Owner.Creature, instance);
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(StrikeSilent __instance)
    {
        __instance.DynamicVars.Damage.UpgradeValueBy(1m);
        __instance.DynamicVars.Poison.UpgradeValueBy(1m);
        return false;
    }
}
