using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.Powersfix;

[HarmonyPatch(typeof(HellraiserPower))]
public static class HellraiserPowerPatch
{
    [HarmonyPatch("AfterCardDrawnEarly")]
    [HarmonyPrefix]
    static bool AfterCardDrawnEarlyPrefix(HellraiserPower __instance, PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ref Task __result)
    {
        __result = PatchAfterCardDrawnEarly(__instance, choiceContext, card, fromHandDraw);
        return false;
    }

    static async Task PatchAfterCardDrawnEarly(HellraiserPower instance, PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner.Creature == instance.Owner && (card.Tags.Contains(CardTag.Strike) || card is Snakebite))
        {
            var autoplayingCardsProperty = AccessTools.Property(typeof(HellraiserPower), "AutoplayingCards");
            dynamic autoplayingCards = autoplayingCardsProperty.GetValue(instance);
            autoplayingCards.Add(card);
            await CardCmd.AutoPlay(choiceContext, card, null);
            autoplayingCards.Remove(card);
        }
    }
}
