using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
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
using SBMod.SBModCode.Powers;


namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(Dismantle))]
public static class DismantlePatch
{
    [HarmonyPatch("ShouldGlowGoldInternal", MethodType.Getter)]
    [HarmonyPostfix]
    static void ShouldGlowGoldPostfix(ref bool __result, Dismantle __instance)
    {
        __result = __instance.CombatState?.HittableEnemies.Any(e => e.HasPower<EasySnakePower>()) ?? false;
    }

    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(Dismantle __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = HoverTipFactory.FromPowerWithPowerHoverTips<EasySnakePower>();
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(Dismantle __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false; 
    }
    static async Task PatchOnPlay(Dismantle instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        int hitCount = (!cardPlay.Target.HasPower<EasySnakePower>()) ? 1 : 2;
        await DamageCmd.Attack(instance.DynamicVars.Damage.BaseValue).WithHitCount(hitCount).FromCard(instance)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }
}

