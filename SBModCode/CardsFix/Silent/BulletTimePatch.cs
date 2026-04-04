using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.CardsFix.Silent;

[HarmonyPatch(typeof(BulletTime))]
public static class BulletTimePatch
{
    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(BulletTime __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(BulletTime instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(instance.Owner.Creature, "Cast", instance.Owner.Character.CastAnimDelay);
        foreach (CardModel card in PileType.Hand.GetPile(instance.Owner).Cards)
        {
            if (card is Snakebite)
            {
                card.SetToFreeThisTurn();
            }
        }
        await PowerCmd.Apply<NoDrawPower>(instance.Owner.Creature, 1m, instance.Owner.Creature, instance);
    }
}
