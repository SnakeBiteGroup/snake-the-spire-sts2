using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(Barrage))]
public static class BarragePatch
{
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(Barrage __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = HoverTipFactory.FromCardWithCardHoverTips<Snakebite>();
    }

    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(Barrage __instance, ref IEnumerable<DynamicVar> __result)
    {
        var vars = __result.ToList();
        for (int i = 0; i < vars.Count; i++)
        {
            if (vars[i] is CalculatedVar calcVar && calcVar.Name == "CalculatedHits")
            {
                vars[i] = new CalculatedVar("CalculatedHits").WithMultiplier((CardModel card, Creature? _) => 
                {
                    var handCards = CardPile.GetCards(card.Owner, PileType.Hand);
                    return handCards.Count(c => c is Snakebite);
                });
            }
        }
        __result = vars;
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(Barrage __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(Barrage instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(instance.DynamicVars.Damage.BaseValue).WithHitCount((int)((CalculatedVar)instance.DynamicVars["CalculatedHits"]).Calculate(cardPlay.Target)).FromCard(instance)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }
}

