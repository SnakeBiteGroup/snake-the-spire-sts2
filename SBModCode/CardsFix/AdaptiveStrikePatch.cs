using System;
using System.Collections.Generic;
using System.Linq;
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

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(AdaptiveStrike))]
public static class AdaptiveStrikePatch
{
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(AdaptiveStrike __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = new List<IHoverTip> { HoverTipFactory.FromPower<PoisonPower>() };
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(AdaptiveStrike __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(AdaptiveStrike instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await PowerCmd.Apply<PoisonPower>(cardPlay.Target, 7m, instance.Owner.Creature, instance);
        CardModel cardModel = instance.CreateClone();
        cardModel.EnergyCost.SetThisCombat(0);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Discard, addedByPlayer: true), 1.5f);
    }

    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(AdaptiveStrike __instance, ref IEnumerable<DynamicVar> __result)
    {
        var vars = __result.ToList();
        for (int i = 0; i < vars.Count; i++)
        {
            if (vars[i] is DamageVar)
            {
                vars[i] = new PowerVar<PoisonPower>(7m);
            }
        }
        __result = vars;
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(AdaptiveStrike __instance)
    {
        __instance.DynamicVars.Poison.UpgradeValueBy(3m);
        return false;
    }
}
