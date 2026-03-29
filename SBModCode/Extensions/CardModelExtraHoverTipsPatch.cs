using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace LOM.SBModCode.CardsFix;

[HarmonyPatch(typeof(CardModel), "ExtraHoverTips", MethodType.Getter)]
public static class CardModelExtraHoverTipsPatch
{
    static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (__instance is AdaptiveStrike || __instance is Shiv || __instance is PerfectedStrike)
        {
            __result = new List<IHoverTip> { HoverTipFactory.FromPower<PoisonPower>() };
        }
    }
}
