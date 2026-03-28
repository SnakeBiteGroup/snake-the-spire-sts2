using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

namespace LOM.SBModCode.CardsFix;

//追踪之剑的效果事实上是在君王之剑卡牌里实现的
[HarmonyPatch(typeof(Snakebite))]
public static class SnakebitePatch
{
    private static bool HasSeekingEdge(Snakebite instance)
    {
        if (CombatManager.Instance.IsInProgress)
        {
            return instance.Owner.Creature.HasPower<SeekingEdgePower>();
        }
        return false;
    }

    private static decimal GetExtraPoisonFromStrikeDummy(Snakebite instance)
    {
        var strikeDummy = instance.Owner?.Relics.OfType<StrikeDummy>().FirstOrDefault();
        if (strikeDummy != null)
        {
            return strikeDummy.DynamicVars["ExtraDamage"].BaseValue;
        }
        return 0m;
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(Snakebite __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(Snakebite instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var extraPoison = GetExtraPoisonFromStrikeDummy(instance);
        var totalPoison = instance.DynamicVars.Poison.BaseValue + extraPoison;
        
        if (HasSeekingEdge(instance))
        {
            await CreatureCmd.TriggerAnim(instance.Owner.Creature, "Cast", instance.Owner.Character.CastAnimDelay);
            var hittableEnemies = instance.CombatState.HittableEnemies;
            foreach (var enemy in hittableEnemies)
            {
                VfxCmd.PlayOnCreatureCenter(enemy, "vfx/vfx_bite");
                await PowerCmd.Apply<PoisonPower>(enemy, totalPoison, instance.Owner.Creature, instance);
            }
        }
        else
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await CreatureCmd.TriggerAnim(instance.Owner.Creature, "Cast", instance.Owner.Character.CastAnimDelay);
            VfxCmd.PlayOnCreatureCenter(cardPlay.Target, "vfx/vfx_bite");
            await PowerCmd.Apply<PoisonPower>(cardPlay.Target, totalPoison, instance.Owner.Creature, instance);
        }
    }
}
