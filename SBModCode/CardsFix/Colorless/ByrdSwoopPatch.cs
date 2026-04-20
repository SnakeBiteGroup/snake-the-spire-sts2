using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SBMod.SBModCode.Enchantments;

namespace SBMod.SBModCode.CardsFix.Colorless;

[HarmonyPatch(typeof(ByrdSwoop))]
public static class ByrdSwoopPatch
{
    [HarmonyPatch("CanonicalVars", MethodType.Getter)]
    [HarmonyPostfix]
    static void CanonicalVarsPostfix(ByrdSwoop __instance, ref IEnumerable<DynamicVar> __result)
    {
        var list = __result?.ToList() ?? new List<DynamicVar>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] is DamageVar)
            {
                list[i] = new DamageVar(8m, ValueProp.Move);
            }
        }
        if (!list.Any(v => v.Name == "PoisonPower"))
        {
            list.Add(new PowerVar<PoisonPower>(5m));
        }
        __result = list;
    }

    [HarmonyPatch("OnPlay")]
    [HarmonyPrefix]
    static bool OnPlayPrefix(ByrdSwoop __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PatchOnPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    static async Task PatchOnPlay(ByrdSwoop instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(instance.DynamicVars.Damage.BaseValue).FromCard(instance).Targeting(cardPlay.Target)
            .WithAttackerAnim("Attack", instance.Owner.Character.AttackAnimDelay, instance.Owner.PlayerCombatState.GetPet<Byrdpip>())
            .WithHitFx("vfx/vfx_attack_slash", "event:/sfx/byrdpip/byrdpip_attack")
            .Execute(choiceContext);
        await PowerCmd.Apply<PoisonPower>(cardPlay.Target, instance.DynamicVars.Poison.BaseValue, instance.Owner.Creature, instance);
    }

    [HarmonyPatch("OnUpgrade")]
    [HarmonyPrefix]
    static bool OnUpgradePrefix(ByrdSwoop __instance)
    {
        __instance.DynamicVars.Damage.UpgradeValueBy(2m);
        __instance.DynamicVars.Poison.UpgradeValueBy(2m);
        return false;
    }
}

[HarmonyPatch(typeof(CombatState), "CreateCard", new[] { typeof(CardModel), typeof(Player) })]
public static class ByrdSwoopEnchantPatch
{
    [HarmonyPostfix]
    static void CreateCardPostfix(CardModel __result)
    {
        if (__result is ByrdSwoop && __result.Enchantment == null)
        {
            CardCmd.Enchant<SerpentStrikeEnchant>(__result, 1);
        }
    }
}
