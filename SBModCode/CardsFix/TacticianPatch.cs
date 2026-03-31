using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(Tactician))]
public static class TacticianPatch
{
    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(Tactician __instance, ref IEnumerable<DynamicVar> __result)
    {
        var list = __result?.ToList() ?? new List<DynamicVar>();
        if (!list.Any(v => v.Name == "Cards"))
        {
            list.Add(new CardsVar(1));
        }
        __result = list;
    }

    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(Tactician __instance, ref IEnumerable<IHoverTip> __result)
    {
        var tips = new List<IHoverTip>();
        tips.Add(Traverse.Create(__instance).Property("EnergyHoverTip").GetValue<IHoverTip>());
        tips.AddRange(HoverTipFactory.FromCardWithCardHoverTips<Snakebite>());
        __result = tips;
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(Tactician __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;

    }

    static async Task PatchOnPlay(Tactician instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(instance.Owner.Creature, "Cast", instance.Owner.Character.CastAnimDelay);
        List<CardModel> cardsToAdd = new List<CardModel>();
        var snakebiteCount = instance.DynamicVars.Cards.BaseValue;
        for (int i = 0; i < snakebiteCount; i++)
        {
            cardsToAdd.Add(instance.CombatState.CreateCard<Snakebite>(instance.Owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(cardsToAdd, PileType.Hand, addedByPlayer: true);
        await PlayerCmd.GainEnergy(instance.DynamicVars.Energy.IntValue, instance.Owner);
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(Tactician __instance)
    {
        __instance.DynamicVars.Cards.UpgradeValueBy(1m);
        __instance.DynamicVars.Energy.UpgradeValueBy(1m);
        return false;
    }
}
