using System;
using System.Collections.Generic;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch]
public static class CardImagePatch
{
    private static readonly HashSet<Type> ModifiedCards = new HashSet<Type>
    {
        typeof(MegaCrit.Sts2.Core.Models.Cards.AdaptiveStrike),
        typeof(MegaCrit.Sts2.Core.Models.Cards.Barrage),
        typeof(MegaCrit.Sts2.Core.Models.Cards.BelieveInYou),
        typeof(MegaCrit.Sts2.Core.Models.Cards.CrashLanding),
        typeof(MegaCrit.Sts2.Core.Models.Cards.Hellraiser),
        typeof(MegaCrit.Sts2.Core.Models.Cards.Orbit),
        typeof(MegaCrit.Sts2.Core.Models.Cards.PerfectedStrike)
    };

    [HarmonyPatch(typeof(CardModel), "HasPortrait", MethodType.Getter)]
    [HarmonyPostfix]
    static void HasPortraitPostfix(CardModel __instance, ref bool __result)
    {
        if (!__result && ModifiedCards.Contains(__instance.GetType()))
        {
            var cardPoolTitle = __instance.Pool.Title.ToLowerInvariant();
            var cardIdEntry = __instance.Id.Entry.ToLowerInvariant();
            var modPortraitPath = ImageHelper.GetImagePath($"packed/card_portraits/{cardPoolTitle}/{cardIdEntry}.png");
            
            if (ResourceLoader.Exists(modPortraitPath))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(CardModel), "PortraitPath", MethodType.Getter)]
    [HarmonyPostfix]
    static void PortraitPathPostfix(CardModel __instance, ref string __result)
    {
        if (ModifiedCards.Contains(__instance.GetType()))
        {
            var cardPoolTitle = __instance.Pool.Title.ToLowerInvariant();
            var cardIdEntry = __instance.Id.Entry.ToLowerInvariant();
            var modPortraitPngPath = ImageHelper.GetImagePath($"packed/card_portraits/{cardPoolTitle}/{cardIdEntry}.png");
            
            if (ResourceLoader.Exists(modPortraitPngPath))
            {
                __result = modPortraitPngPath;
            }
        }
    }
}
