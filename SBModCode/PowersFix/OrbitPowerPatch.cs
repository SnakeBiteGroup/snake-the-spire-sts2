using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SBMod.SBModCode.CardsFix;

[HarmonyPatch(typeof(OrbitPower), "AfterEnergySpent")]
public static class OrbitPowerPatch
{
    static bool Prefix(OrbitPower __instance, CardModel card, int amount, ref Task __result)
    {
        __result = PatchAfterEnergySpent(__instance, card, amount);
        return false;
    }

    static async Task PatchAfterEnergySpent(OrbitPower instance, CardModel card, int amount)
    {
        if (card.Owner.Creature == instance.Owner && amount > 0)
        {
            var getInternalDataMethod = AccessTools.Method(typeof(OrbitPower), "GetInternalData", System.Type.EmptyTypes);
            dynamic data = getInternalDataMethod.Invoke(instance, null);
            data.energySpent += amount;
            int triggers = data.energySpent / 4 - data.triggerCount;
            if (triggers > 0)
            {
                var flashMethod = AccessTools.Method(typeof(PowerModel), "Flash", System.Type.EmptyTypes);
                flashMethod.Invoke(instance, null);
                List<CardModel> cardsToAdd = new List<CardModel>();
                for (int i = 0; i < triggers; i++)
                {
                    cardsToAdd.Add(instance.Owner.CombatState.CreateCard<Snakebite>(instance.Owner.Player));
                }
                await CardPileCmd.AddGeneratedCardsToCombat(cardsToAdd, PileType.Hand, addedByPlayer: true);
                data.triggerCount += triggers;
            }
            var invokeDisplayMethod = AccessTools.Method(typeof(OrbitPower), "InvokeDisplayAmountChanged", System.Type.EmptyTypes);
            invokeDisplayMethod.Invoke(instance, null);
        }
    }
}