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

[HarmonyPatch(typeof(MoltenFist))]
public static class MoltenFistPatch
{
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(MoltenFist __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = HoverTipFactory.FromPowerWithPowerHoverTips<EasySnakePower>();
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(Bash __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }
    static async Task PatchOnPlay(Bash instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

       ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(instance.DynamicVars.Damage.BaseValue).FromCard(instance).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_molten_fist", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        int num = cardPlay.Target.IsAlive ? cardPlay.Target.GetPowerAmount<EasySnakePower>() : 0;
        if (num > 0)
        {
            await PowerCmd.Apply<EasySnakePower>(cardPlay.Target, num,instance.Owner.Creature, instance);
        }
    }
}

