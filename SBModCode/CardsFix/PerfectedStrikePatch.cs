using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(PerfectedStrike))]
public static class PerfectedStrikePatch
{
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(PerfectedStrike __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = new List<IHoverTip> { HoverTipFactory.FromPower<PoisonPower>() };
    }

    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(PerfectedStrike __instance, ref IEnumerable<DynamicVar> __result)
    {
        __result = new DynamicVar[2]
        {
            new PowerVar<PoisonPower>(7m),
            new ExtraDamageVar(3m)
        };
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
        int poisonAmount = (int)instance.DynamicVars.Poison.BaseValue + (snakebiteCount * (int)instance.DynamicVars.ExtraDamage.BaseValue);
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
