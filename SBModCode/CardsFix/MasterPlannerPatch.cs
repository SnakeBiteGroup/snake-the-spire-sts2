using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(MasterPlanner))]
public static class MasterPlannerPatch
{
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(MasterPlanner __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = HoverTipFactory.FromCardWithCardHoverTips<Snakebite>();
    }
}