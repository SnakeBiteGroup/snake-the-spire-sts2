using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.Extensions;

[HarmonyPatch(typeof(PowerModel), "ExtraHoverTips", MethodType.Getter)]
public static class PowerModelExtraHoverTipsPatch
{
    static void Postfix(PowerModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (__instance is MasterPlannerPower)
        {
            __result = new List<IHoverTip> (HoverTipFactory.FromCardWithCardHoverTips<Snakebite>());
        }
    }
}
