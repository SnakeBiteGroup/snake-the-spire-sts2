using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(DramaticEntrance))]
public static class DramaticEntrancePatch
{
    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(DramaticEntrance __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }
    static async Task PatchOnPlay(DramaticEntrance instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(instance.Owner.Creature, "Attack", instance.Owner.Character.AttackAnimDelay);
        VfxCmd.PlayFullScreenInCombat("vfx/vfx_dramatic_entrance_fullscreen");
        var totalPoison = instance.DynamicVars.Damage.BaseValue;
        if (instance.CombatState == null) throw new Exception("Error: 蛇咬登场 PatchOnPlay CombatState 为空");
        foreach (Creature hittableEnemy in instance.CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<PoisonPower>(hittableEnemy, totalPoison, instance.Owner.Creature, instance);
        }
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(DramaticEntrance __instance)
    {
        __instance.DynamicVars.Damage.UpgradeValueBy(4m);
        return false;
    }
}
