using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.CardsFix.Regent;

[HarmonyPatch(typeof(Orbit))]
public static class OrbitPatch
{
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(Orbit __instance, ref IEnumerable<IHoverTip> __result)
    {
        var list = new List<IHoverTip>(__result);
        list.AddRange(HoverTipFactory.FromCardWithCardHoverTips<Snakebite>());
        __result = list;
    }
}
