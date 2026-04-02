using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(HeavenlyDrill))]
public class HeavenlyDrillPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(MethodType.Constructor)]
    static void PostfixConstructor(HeavenlyDrill __instance)
    {
        __instance.AddKeyword(CardKeyword.Retain);
        __instance.DynamicVars.Damage.BaseValue = 4m;
    }
    
    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(GrandFinale __instance, ref IEnumerable<DynamicVar> __result)
    {
        __result = new List<DynamicVar>
        {
            new PowerVar<PoisonPower>(4m)
        };
    }
    
    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(Flechettes __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return true;
    }

    static async Task PatchOnPlay(Flechettes instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int num = instance.ResolveEnergyXValue();
        if (num >= instance.DynamicVars.Energy.IntValue)
        {
            num *= 2;
        }
        for (int i = 0; i < num; i++)
        {
            VfxCmd.PlayOnCreatureCenter(cardPlay.Target, "vfx/vfx_bite");
            await PowerCmd.Apply<PoisonPower>(cardPlay.Target, instance.DynamicVars.Poison.BaseValue,
                instance.Owner.Creature, instance);
        }
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(AdaptiveStrike __instance)
    {
        __instance.DynamicVars.Poison.UpgradeValueBy(2m);
        return true;
    }
}