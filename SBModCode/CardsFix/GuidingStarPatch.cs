using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using SBMod.SBModCode.Powers;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(GuidingStar))]
public static class GuidingStarPatch
{
    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(GuidingStar __instance, ref IEnumerable<DynamicVar> __result)
    {
        var list = __result?.ToList() ?? new List<DynamicVar>();
        list.Add(new PowerVar<PoisonPower>(12m));
        __result = list;
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(GuidingStar __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(GuidingStar instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.TriggerAnim(instance.Owner.Creature, "Cast", instance.Owner.Character.CastAnimDelay);
        VfxCmd.PlayOnCreatureCenter(cardPlay.Target, "vfx/vfx_bite");
        await PowerCmd.Apply<PoisonPower>(cardPlay.Target, instance.DynamicVars.Poison.BaseValue, instance.Owner.Creature, instance);
        
        await PowerCmd.Apply<SnakebiteGuidePower>(instance.Owner.Creature, (int)instance.DynamicVars.Cards.BaseValue, instance.Owner.Creature, instance);
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(GuidingStar __instance)
    {
        __instance.DynamicVars.Poison.UpgradeValueBy(3m);
        __instance.DynamicVars.Cards.UpgradeValueBy(1m);
        return false;
    }
}
