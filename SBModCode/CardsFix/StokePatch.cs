using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using SBMod.SBModCode.Extensions;


namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(Stoke))]
public static class StokePatch
{
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(PrimalForce __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = HoverTipFactory.FromCardWithCardHoverTips<Snakebite>(__instance.IsUpgraded);
    }
    
    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(Stoke __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }
    static async Task PatchOnPlay(Stoke instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        var owner = instance.Owner;
        var state = instance.CombatState;

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);
        List<CardModel> list = PileType.Hand.GetPile(owner).Cards.ToList();
        int exhaustCount = list.Count;
        foreach (CardModel item in list)
        {
            await CardCmd.Exhaust(choiceContext, item);
        }

        /*var allCards = owner.Character.CardPool
    .GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint);

        var snakebitePool = allCards
            .Where(c => SnakebiteHelper.IsSnakebiteCard(c))
            .ToList();

        var cards = CardFactory.GetForCombat(
            owner,
            snakebitePool,
            exhaustCount,
            owner.RunState.Rng.CombatCardGeneration
        ).ToList();*/
        for (int i = 0; i < list.Count; i++)
        {
            var card = state.CreateCard<Snakebite>(owner);
            if (instance.IsUpgraded)
            {
                CardCmd.Upgrade(card, CardPreviewStyle.None);
            }
            await CardPileCmd.AddGeneratedCardsToCombat([card], PileType.Hand, addedByPlayer: true);
        }
    }
}

