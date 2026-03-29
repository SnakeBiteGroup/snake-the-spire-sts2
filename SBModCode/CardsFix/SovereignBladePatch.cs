using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace LOM.SBModCode.CardsFix;

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
        await CreatureCmd.TriggerAnim(instance.Owner.Creature, "Cast", instance.Owner.Character.AttackAnimDelay);

        int poisonAmount = (int)instance.DynamicVars.Damage.BaseValue;
        int hitCount = instance.DynamicVars.Repeat.IntValue;
        int totalPoison = poisonAmount * hitCount;

        if (HasSeekingEdge(instance))
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

            foreach (var enemy in hittableEnemies)
            {
                VfxCmd.PlayOnCreatureCenter(enemy, "vfx/vfx_giant_horizontal_slash");
                for (int i = 0; i < hitCount; i++)
                {
                    await PowerCmd.Apply<PoisonPower>(enemy, poisonAmount, instance.Owner.Creature, instance);
                }
            }
        }
        else
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            NSovereignBladeVfx vfxNode = SovereignBlade.GetVfxNode(instance.Owner, instance);
            NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
            if (vfxNode != null && nCreature != null)
            {
                vfxNode.Attack(nCreature.VfxSpawnPosition);
            }

            VfxCmd.PlayOnCreatureCenter(cardPlay.Target, "vfx/vfx_giant_horizontal_slash");
            for (int i = 0; i < hitCount; i++)
            {
                await PowerCmd.Apply<PoisonPower>(cardPlay.Target, poisonAmount, instance.Owner.Creature, instance);
            }
        }

        ParryPower power = instance.Owner.Creature.GetPower<ParryPower>();
        if (power != null)
        {
            await power.AfterSovereignBladePlayed(instance.Owner.Creature, null);
        }
    }
}
