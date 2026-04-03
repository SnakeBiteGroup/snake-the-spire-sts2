using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.Extensions;

[HarmonyPatch(typeof(CardModel), "ExtraHoverTips", MethodType.Getter)]
public static class CardModelExtraHoverTipsPatch
{
    static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (__instance is AdaptiveStrike 
            || __instance is Shiv 
            || __instance is PerfectedStrike
            || __instance is SovereignBlade
            || __instance is GuidingStar
            || __instance is ShiningStrike
            || __instance is DaggerThrow
            || __instance is Radiate
            || __instance is DramaticEntrance
            || __instance is SerpentForm
            || __instance is DaggerThrow
            || __instance is Flechettes
            || __instance is GrandFinale
            || __instance is HeavenlyDrill
            )
            
        {
            __result = new List<IHoverTip> { HoverTipFactory.FromPower<PoisonPower>() };
        }
        if (__instance is SpoilsMap
            || __instance is GrandFinale
            || __instance is Normality
            || __instance is SerpentForm
            || __instance is GuidingStar
            || __instance is BulletTime
            || __instance is DaggerThrow
            || __instance is Radiate
            || __instance is Flechettes
            )
        {
            __result =  HoverTipFactory.FromCardWithCardHoverTips<Snakebite>() ;
        }
    }
}
