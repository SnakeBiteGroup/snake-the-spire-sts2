using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(SovereignBlade))]
public static class SovereignBladePatch
{
    private static bool HasSeekingEdge(SovereignBlade instance)
    {
        if (CombatManager.Instance.IsInProgress)
        {
            return instance.Owner.Creature.HasPower<SeekingEdgePower>();
        }
        return false;
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(SovereignBlade __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(SovereignBlade instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        AttackCommand attack = DamageCmd.Attack(instance.DynamicVars.Damage.BaseValue).FromCard(instance).WithHitCount(instance.DynamicVars.Repeat.IntValue)
            .WithAttackerAnim("Cast", instance.Owner.Character.AttackAnimDelay)
            .WithAttackerFx(null, "event:/sfx/characters/regent/regent_sovereign_blade");
        if (HasSeekingEdge(instance))
        {
            attack = attack.TargetingAllOpponents(instance.CombatState).BeforeDamage(delegate
            {
                NSovereignBladeVfx vfxNode = SovereignBlade.GetVfxNode(instance.Owner, instance);
                IReadOnlyList<Creature> hittableEnemies = instance.CombatState.HittableEnemies;
                if (hittableEnemies.Count > 0)
                {
                    NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(hittableEnemies[0]);
                    if (vfxNode != null && nCreature != null)
                    {
                        vfxNode.Attack(nCreature.VfxSpawnPosition);
                    }
                }

                return Task.CompletedTask;
            }).WithHitFx("vfx/vfx_giant_horizontal_slash", null, "slash_attack.mp3");
        }
        else
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            attack = attack.Targeting(cardPlay.Target).BeforeDamage(delegate
            {
                NSovereignBladeVfx vfxNode = SovereignBlade.GetVfxNode(instance.Owner, instance);
                NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
                if (vfxNode != null && nCreature != null)
                {
                    vfxNode.Attack(nCreature.VfxSpawnPosition);
                }

                return Task.CompletedTask;
            }).WithHitVfxNode((Creature t) => NBigSlashVfx.Create(t))
                .WithHitVfxNode((Creature t) => NBigSlashImpactVfx.Create(t));
        }

        await attack.Execute(choiceContext);

        await PowerCmd.Apply<PoisonPower>(cardPlay.Target, attack.Results.Sum((DamageResult r) => r.TotalDamage + r.OverkillDamage), instance.Owner.Creature, instance);


        ParryPower power = instance.Owner.Creature.GetPower<ParryPower>();
        if (power != null)
        {
            await power.AfterSovereignBladePlayed(instance.Owner.Creature, null);
        }
    }
}
