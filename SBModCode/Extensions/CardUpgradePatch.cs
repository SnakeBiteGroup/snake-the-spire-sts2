using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.Extensions;

[HarmonyPatch(typeof(CardModel), "OnUpgrade")]
public class CardUpgradePatch
{
    static void Postfix(CardModel __instance)
    {
        if (__instance is SpoilsMap)
        {
            int currentBase = (int)__instance.DynamicVars.Gold.BaseValue;
            __instance.DynamicVars.Gold.UpgradeValueBy(currentBase);
        }
    }
}