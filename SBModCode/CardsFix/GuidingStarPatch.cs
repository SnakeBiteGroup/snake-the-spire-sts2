using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using SBMod.SBModCode.Powers;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(GuidingStar))]
public static class GuidingStarPatch
{
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPostfix]
    static void ConstructorPostfix(GuidingStar __instance)
    {
        var typeProperty = AccessTools.Property(typeof(CardModel), "Type");
        if (typeProperty?.SetMethod != null)
        {
            typeProperty.SetValue(__instance, CardType.Skill);
            return;
        }

        var typeField = AccessTools.Field(typeof(CardModel), "<Type>k__BackingField");
        typeField?.SetValue(__instance, CardType.Skill);
    }
    
    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(GuidingStar __instance, ref IEnumerable<DynamicVar> __result)
    {
        var list = __result?.ToList() ?? new List<DynamicVar>();
        list.Add(new PowerVar<PoisonPower>(12m));
        __result = list;
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(GuidingStar __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(GuidingStar instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.TriggerAnim(instance.Owner.Creature, "Cast", instance.Owner.Character.CastAnimDelay);
        VfxCmd.PlayOnCreatureCenter(cardPlay.Target, "vfx/vfx_bite");
        await PowerCmd.Apply<PoisonPower>(cardPlay.Target, instance.DynamicVars.Poison.BaseValue, instance.Owner.Creature, instance);
        
        await PowerCmd.Apply<DrawSnakebitesNextTurnPower>(instance.Owner.Creature, (int)instance.DynamicVars.Cards.BaseValue, instance.Owner.Creature, instance);
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(GuidingStar __instance)
    {
        __instance.DynamicVars.Poison.UpgradeValueBy(3m);
        __instance.DynamicVars.Cards.UpgradeValueBy(1m);
        return false;
    }
}
