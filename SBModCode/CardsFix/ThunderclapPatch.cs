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
using MegaCrit.Sts2.Core.ValueProps;
using SBMod.SBModCode.Powers;


namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(Thunderclap))]
public static class ThunderclapPatch
{
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(Thunderclap __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = HoverTipFactory.FromPowerWithPowerHoverTips<EasySnakePower>();
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(Thunderclap __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }
    static async Task PatchOnPlay(Thunderclap instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

       await DamageCmd.Attack(instance.DynamicVars.Damage.BaseValue).FromCard(instance).TargetingAllOpponents(instance.CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await PowerCmd.Apply<EasySnakePower>(instance.CombatState.HittableEnemies, instance.DynamicVars.Vulnerable.BaseValue, instance.Owner.Creature, instance);
    
    }
}

