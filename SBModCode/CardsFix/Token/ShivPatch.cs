using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Godot;

namespace SBMod.SBModCode.CardsFix.Token;

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
        AttackCommand attackCommand = DamageCmd.Attack(instance.DynamicVars.Damage.BaseValue).FromCard(instance);
        var totalPoison = instance.DynamicVars.Poison.BaseValue;
        if (CombatManager.Instance.IsInProgress && instance.Owner != null)
        {
            hasFanOfKnives = instance.Owner.Creature.HasPower<FanOfKnivesPower>();
        }

        if (hasFanOfKnives)
        {
            var hittableEnemies = instance.CombatState.HittableEnemies;
            Creature lastEnemy = hittableEnemies.LastOrDefault();
            attackCommand = attackCommand.TargetingAllOpponents(instance.CombatState).WithHitVfxNode(_ => NShivThrowVfx.Create(instance.Owner.Creature, lastEnemy, Colors.Green));
            foreach (var enemy in hittableEnemies)
            {
                await PowerCmd.Apply<PoisonPower>(choiceContext,enemy, totalPoison, instance.Owner.Creature, instance);
            }
        }
        else
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await PowerCmd.Apply<PoisonPower>(choiceContext,cardPlay.Target, totalPoison, instance.Owner.Creature, instance);
        }
        if (instance.Owner.Character is MegaCrit.Sts2.Core.Models.Characters.Silent)
        {
            attackCommand.WithAttackerAnim("Shiv", 0.2f);
        }
        await attackCommand.Execute(choiceContext);
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(Shiv __instance)
    {
        __instance.DynamicVars.Damage.UpgradeValueBy(1m);
        __instance.DynamicVars.Poison.UpgradeValueBy(1m);
        return false;
    }
}