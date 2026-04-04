using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.CardsFix.Regent;

[HarmonyPatch(typeof(Radiate))]
public static class RadiatePatch
{
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPostfix]
    static void ConstructorPostfix(Radiate __instance)
    {
        var typeField = AccessTools.Field(typeof(CardModel), "<Type>k__BackingField");
        typeField?.SetValue(__instance, CardType.Skill);
    }

    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(Radiate __instance, ref IEnumerable<DynamicVar> __result)
    {
        var vars = __result.ToList();
        for (int i = 0; i < vars.Count; i++)
        {
            if (vars[i] is CalculatedVar calcVar && calcVar.Name == "CalculatedHits")
            {
                vars[i] = new CalculatedVar("CalculatedHits").WithMultiplier((CardModel card, Creature? _) => 
                    CombatManager.Instance.History.Entries.OfType<CardPlayFinishedEntry>()
                        .Count(e => e.HappenedThisTurn(card.CombatState) && e.CardPlay.Card is Snakebite));
            }
        }
        vars.Add(new PowerVar<PoisonPower>(3m));
        __result = vars;
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(Radiate __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(Radiate instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int hitCount = CombatManager.Instance.History.Entries.OfType<CardPlayFinishedEntry>()
            .Count(e => e.HappenedThisTurn(instance.CombatState) && e.CardPlay.Card is Snakebite);
        
        for (int i = 0; i < hitCount; i++)
        {
            foreach (var enemy in instance.CombatState.HittableEnemies)
            {
                VfxCmd.PlayOnCreatureCenter(enemy, "vfx/vfx_bite");
                await PowerCmd.Apply<PoisonPower>(enemy, instance.DynamicVars.Poison.BaseValue, instance.Owner.Creature, instance);
            }
        }
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(Radiate __instance)
    {
        __instance.DynamicVars.Poison.UpgradeValueBy(1m);
        return false;
    }
}
