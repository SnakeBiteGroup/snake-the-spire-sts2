using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.PowersFix;

[HarmonyPatch(typeof(StranglePower))]
public static class StranglePowerPatch
{
    [HarmonyPatch("AfterCardPlayed")]
    [HarmonyPrefix]
    static bool AfterCardPlayedPrefix(StranglePower __instance, PlayerChoiceContext context, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchAfterCardPlayed(__instance, context, cardPlay);
        return false;
    }

    static async Task PatchAfterCardPlayed(StranglePower instance, PlayerChoiceContext context, CardPlay cardPlay)
    {
        var internalData = Traverse.Create(instance).Field("_internalData").GetValue();
        var dataTraverse = Traverse.Create(internalData);
        var amountsForPlayedCards = dataTraverse.Field("amountsForPlayedCards").GetValue<Dictionary<CardModel, int>>();
        
        if (amountsForPlayedCards.Remove(cardPlay.Card, out var value))
        {
            Traverse.Create(instance).Method("Flash").GetValue();
            
            {
                VfxCmd.PlayOnCreatureCenter(instance.Owner,  "vfx/vfx_bite");
                await PowerCmd.Apply<PoisonPower>(instance.Owner, instance.Amount, instance.Owner, null);
            }
        }
    }
}