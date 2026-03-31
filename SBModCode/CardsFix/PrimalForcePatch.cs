using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;


namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(PrimalForce))]
public static class PrimalForcePatch
{
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(PrimalForce __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = HoverTipFactory.FromCardWithCardHoverTips<Snakebite>(__instance.IsUpgraded);
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(PrimalForce __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }
    static async Task PatchOnPlay(PrimalForce instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        var owner = instance.Owner;
        var state = instance.CombatState;

        if (state == null || owner == null)
            return;

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);

        var hand = PileType.Hand.GetPile(owner).Cards
            .Where(c => c != null && c.IsTransformable && c.Type == CardType.Skill)
            .ToList();

        if (hand.Count == 0)
            return;

        foreach (var original in hand)
        {
            CardModel newCard = state.CreateCard<Snakebite>(owner); 
            if (instance.IsUpgraded && newCard != null)
                CardCmd.Upgrade(newCard);

            if (newCard != null)
                await CardCmd.Transform(original, newCard);
        }
    }
}

