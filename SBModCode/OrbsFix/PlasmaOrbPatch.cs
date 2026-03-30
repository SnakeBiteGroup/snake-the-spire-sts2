using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace SBMod.SBModCode.OrbsFix;

[HarmonyPatch(typeof(PlasmaOrb))]
public static class PlasmaOrbPatch
{
    [HarmonyPatch("DarkenedColor", MethodType.Getter)]
    [HarmonyPostfix]
    static void DarkenedColorPostfix(PlasmaOrb __instance, ref Color __result)
    {
        __result = new Color("008000");
    }

    [HarmonyPatch("Passive")]
    [HarmonyPrefix]
    static bool PassivePrefix(PlasmaOrb __instance, PlayerChoiceContext choiceContext, Creature? target, ref Task __result)
    {
        __result = PatchPassive(__instance, choiceContext, target);
        return false;
    }

    static async Task PatchPassive(PlasmaOrb instance, PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target != null)
        {
            throw new InvalidOperationException("Plasma orbs cannot target creatures.");
        }
        instance.Trigger();
        await PlayerCmd.GainEnergy(instance.PassiveVal, instance.Owner);
        
        List<CardModel> cardsToAdd = new List<CardModel>();
        cardsToAdd.Add(instance.CombatState.CreateCard<Snakebite>(instance.Owner));
        await CardPileCmd.AddGeneratedCardsToCombat(cardsToAdd, PileType.Hand, addedByPlayer: true);
    }

    [HarmonyPatch("Evoke")]
    [HarmonyPrefix]
    static bool EvokePrefix(PlasmaOrb __instance, PlayerChoiceContext playerChoiceContext, ref Task<IEnumerable<Creature>> __result)
    {
        __result = PatchEvoke(__instance, playerChoiceContext);
        return false;
    }

    static async Task<IEnumerable<Creature>> PatchEvoke(PlasmaOrb instance, PlayerChoiceContext playerChoiceContext)
    {
        Traverse.Create(instance).Method("PlayEvokeSfx").GetValue();
        await PlayerCmd.GainEnergy(instance.EvokeVal, instance.Owner);
        
        List<CardModel> cardsToAdd = new List<CardModel>();
        for (int i = 0; i < 2; i++)
        {
            cardsToAdd.Add(instance.CombatState.CreateCard<Snakebite>(instance.Owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(cardsToAdd, PileType.Hand, addedByPlayer: true);
        
        return new List<Creature> { instance.Owner.Creature };
    }
}
