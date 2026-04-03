using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(HeavenlyDrill))]
public class HeavenlyDrillPatch
{
    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(HeavenlyDrill __instance, ref IEnumerable<DynamicVar> __result)
    {
        __result = new List<DynamicVar>
        {
            new DamageVar(4m, ValueProp.Move),
            new PowerVar<PoisonPower>(4m),
            new EnergyVar(4)
        };
    }
    
    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(HeavenlyDrill __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return true;
    }

    static async Task PatchOnPlay(HeavenlyDrill instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
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
    static bool OnUpgradePrefix(HeavenlyDrill __instance)
    {
        __instance.DynamicVars.Poison.UpgradeValueBy(2m);
        return true;
    }
}