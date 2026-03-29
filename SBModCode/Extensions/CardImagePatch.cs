using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.Extensions;

[HarmonyPatch]
public static class CardImagePatch
{
    private static readonly HashSet<Type> ModifiedCards = new HashSet<Type>
    {
        typeof(AdaptiveStrike),
        typeof(Barrage),
        typeof(BelieveInYou),
        typeof(CrashLanding),
        typeof(Hellraiser),
        typeof(Orbit),
        typeof(PerfectedStrike),
        typeof(MasterPlanner),
        typeof(SeekingEdge),
        typeof(DoubleEnergy),
        typeof(HiddenDaggers),
        typeof(FanOfKnives),
        typeof(Shiv)
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
