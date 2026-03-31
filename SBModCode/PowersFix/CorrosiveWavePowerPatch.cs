using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.PowersFix;

[HarmonyPatch(typeof(CorrosiveWavePower))]
public static class CorrosiveWavePowerPatch
{
    [HarmonyPatch("AfterCardDrawn")]
    [HarmonyPrefix]
    static bool AfterCardDrawnPrefix(CorrosiveWavePower __instance, PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ref Task __result)
    {
        __result = PatchAfterCardDrawn(__instance, choiceContext, card, fromHandDraw);
        return false;
    }

    static async Task PatchAfterCardDrawn(CorrosiveWavePower instance, PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner?.Creature != instance.Owner)
        {
            return;
        }
        
        if (card is Snakebite)
        {
            Traverse.Create(instance).Method("Flash").GetValue();
            Creature? target = instance.CombatState.HittableEnemies.FirstOrDefault();
            if (target != null)
            {
                await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, skipXCapture: false, true);
            }
        }
    }
}
