using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(Flechettes))]
public static class FlechettesPatch
{
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPostfix]
    static void ConstructorPostfix(Flechettes __instance)
    {
        var typeField = AccessTools.Field(typeof(CardModel), "<Type>k__BackingField");
        typeField?.SetValue(__instance, CardType.Skill);
    }

    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(Flechettes __instance, ref IEnumerable<DynamicVar> __result)
    {
        var list = __result?.ToList() ?? new List<DynamicVar>();
            list.Add(new PowerVar<PoisonPower>(5m));
        __result = list;
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(Flechettes __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(Flechettes instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        
        var handCards = CardPile.GetCards(instance.Owner, PileType.Hand);
        int snakebiteCount = handCards.Count(c => c is Snakebite);
        var poisonAmount = instance.DynamicVars.Poison.BaseValue;
        
        await CreatureCmd.TriggerAnim(instance.Owner.Creature, "Cast", instance.Owner.Character.AttackAnimDelay);
        
        // 为每张蛇咬单独播放动画并施加中毒
        for (int i = 0; i < snakebiteCount; i++)
        {
            VfxCmd.PlayOnCreatureCenter(cardPlay.Target, "vfx/vfx_bite");
            await PowerCmd.Apply<PoisonPower>(cardPlay.Target, poisonAmount, instance.Owner.Creature, instance);
            await Task.Delay(200);
        }
    }
    
    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(Flechettes __instance)
    {
        __instance.DynamicVars.Poison.UpgradeValueBy(2m);
        return false;
    }
}
