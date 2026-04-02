using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.Extensions;

[HarmonyPatch(typeof(CardModel), "CanonicalKeywords", MethodType.Getter)]
public static class CardModelKeywordsPatch
{
    static void Postfix(CardModel __instance, ref IEnumerable<CardKeyword> __result)
    {
        if (__instance is HeavenlyDrill)
        {
            __result = new List<CardKeyword> { CardKeyword.Retain };
        }
    }
}