using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.PowersFix;

[HarmonyPatch(typeof(OrbitPower))]
public static class OrbitPowerPatch
{
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(OrbitPower __instance, ref IEnumerable<IHoverTip> __result)
    {
        var list = new List<IHoverTip>();
        list.AddRange(HoverTipFactory.FromCardWithCardHoverTips<Snakebite>());
        __result = list;
    }

    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(OrbitPower __instance, ref IEnumerable<DynamicVar> __result)
    {
        __result = new DynamicVar[1]
        {
            new EnergyVar(4)
        };
    }

    [HarmonyPatch("DisplayAmount", MethodType.Getter)]
    [HarmonyPostfix]
    static void DisplayAmountPostfix(OrbitPower __instance, ref int __result)
    {
        var internalData = Traverse.Create(__instance).Field("_internalData").GetValue();
        var dataTraverse = Traverse.Create(internalData);
        int triggerCount = dataTraverse.Field("triggerCount").GetValue<int>();
        int initialAmount = (int)__instance.DynamicVars.Energy.BaseValue;
        __result = initialAmount - triggerCount;
    }

    [HarmonyPatch("AfterEnergySpent")]
    [HarmonyPrefix]
    static bool AfterEnergySpentPrefix(OrbitPower __instance, CardModel card, int amount, ref Task __result)
    {
        __result = PatchAfterEnergySpent(__instance, card, amount);
        return false;
    }

    static async Task PatchAfterEnergySpent(OrbitPower instance, CardModel card, int amount)
    {
        if (card.Owner.Creature == instance.Owner && amount > 0 && card is Snakebite)
        {
            var instanceTraverse = Traverse.Create(instance);
            var internalData = Traverse.Create(instance).Field("_internalData").GetValue();
            var dataTraverse = Traverse.Create(internalData);

            int triggerCount = dataTraverse.Field("triggerCount").GetValue<int>();
            int initialAmount = (int)instance.DynamicVars.Energy.BaseValue;

            triggerCount++;
            dataTraverse.Field("triggerCount").SetValue(triggerCount);

            if (triggerCount >= initialAmount)
            {
                instanceTraverse.Method("Flash").GetValue();
                var snakebite = instance.Owner.CombatState.CreateCard<Snakebite>(instance.Owner.Player);
                await CardPileCmd.AddGeneratedCardToCombat(snakebite, PileType.Hand, addedByPlayer: true);
                dataTraverse.Field("triggerCount").SetValue(0);
            }

            instanceTraverse.Method("InvokeDisplayAmountChanged").GetValue();
        }
    }
}
