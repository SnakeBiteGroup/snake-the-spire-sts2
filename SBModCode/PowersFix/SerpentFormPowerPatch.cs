using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.PowersFix;

[HarmonyPatch(typeof(SerpentFormPower))]
public static class SerpentFormPowerPatch
{
    [HarmonyPatch("AfterCardPlayed")]
    [HarmonyPrefix]
    static bool AfterCardPlayedPrefix(SerpentFormPower __instance, PlayerChoiceContext context, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchAfterCardPlayed(__instance, context, cardPlay);
        return false;
    }

    static async Task PatchAfterCardPlayed(SerpentFormPower instance, PlayerChoiceContext context, CardPlay cardPlay)
    {
        var internalData = Traverse.Create(instance).Field("_internalData").GetValue();
        var dataTraverse = Traverse.Create(internalData);
        var amountsForPlayedCards = dataTraverse.Field("amountsForPlayedCards").GetValue<Dictionary<CardModel, int>>();
        
        if (cardPlay.Card.Owner == instance.Owner.Player && amountsForPlayedCards.Remove(cardPlay.Card, out var damage) && damage > 0)
        {
            await Cmd.CustomScaledWait(0.1f, 0.2f);
            Creature creature = instance.Owner.Player.RunState.Rng.CombatTargets.NextItem(instance.Owner.CombatState.HittableEnemies);
            if (creature != null)
            {
                VfxCmd.PlayOnCreatureCenter(creature, "vfx/vfx_bite");
                await PowerCmd.Apply<PoisonPower>(creature, damage, instance.Owner, null);
            }
        }
    }
}
