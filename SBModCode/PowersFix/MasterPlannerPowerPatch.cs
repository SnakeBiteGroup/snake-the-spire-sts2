using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace SBMod.SBModCode.PowersFix;

[HarmonyPatch(typeof(MasterPlannerPower))]
public static class MasterPlannerPowerPatch
{
    [HarmonyPatch(typeof(MasterPlannerPower), "AfterCardPlayed")]
    [HarmonyPrefix]
    static bool AfterCardPlayedLatePrefix(MasterPlannerPower __instance, PlayerChoiceContext context, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchAfterCardPlayedLate(__instance, context, cardPlay);
        return false; // 跳过原方法
    }

    static async Task PatchAfterCardPlayedLate(MasterPlannerPower instance, PlayerChoiceContext context, CardPlay cardPlay)
    {

        CardModel card = cardPlay.Card;
        if (card.Owner != instance.Owner.Player) return;
        if (card.Type != CardType.Skill) return;
        if (card is Snakebite) return;
        if (instance.Owner.CombatState == null) return;

        var snakebite = instance.Owner.CombatState.CreateCard<Snakebite>(instance.Owner.Player);
        if (cardPlay.Card.IsUpgraded) CardCmd.Upgrade(snakebite);
        await CardCmd.Discard(context, card);
        await CardCmd.Transform(card, snakebite);
    }
}
