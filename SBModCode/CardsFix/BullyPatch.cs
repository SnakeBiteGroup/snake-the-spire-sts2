using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SBMod.SBModCode.Powers;


namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(Bully))]
public static class BullyPatch
{
    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(Bully __instance, ref IEnumerable<DynamicVar> __result)
    {
        var vars = __result.ToList();
        for (int i = 0; i < vars.Count; i++)
        {
            if (vars[i] is CalculatedDamageVar calcVar)
            {
                vars[i] = new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
                   _?.GetPowerAmount<EasySnakePower>() ?? 0);
            }
        }
        __result = vars;
    }
    
    [HarmonyPatch("ExtraHoverTips", MethodType.Getter)]
    [HarmonyPostfix]
    static void ExtraHoverTipsPostfix(Bully __instance, ref IEnumerable<IHoverTip> __result)
    {
        __result = HoverTipFactory.FromPowerWithPowerHoverTips<EasySnakePower>();
    }
}

