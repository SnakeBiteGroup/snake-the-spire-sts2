using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(Shiv))]
public static class ShivPatch
{
    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(Shiv __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(Shiv instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hasFanOfKnives = false;
        if (CombatManager.Instance.IsInProgress && instance.Owner != null)
        {
            hasFanOfKnives = instance.Owner.Creature.HasPower<FanOfKnivesPower>();
        }

        var totalPoison = instance.DynamicVars.Damage.BaseValue;

        if (hasFanOfKnives)
        {
            var hittableEnemies = instance.CombatState.HittableEnemies;
            foreach (var enemy in hittableEnemies)
            {
                await PowerCmd.Apply<PoisonPower>(enemy, totalPoison, instance.Owner.Creature, instance);
            }
        }
        else
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await PowerCmd.Apply<PoisonPower>(cardPlay.Target, totalPoison, instance.Owner.Creature, instance);
        }
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(Shiv __instance)
    {
        __instance.DynamicVars.Damage.UpgradeValueBy(2m);
        return false;
    }
}
