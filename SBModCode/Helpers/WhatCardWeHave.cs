using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMOD.SBModCode.Helpers;

public static class WhatCardWeHave
{
    public static List<CardModel> GetAllModifiedCards(CombatState combatState, Player owner)
    {
        var cards = new List<CardModel>();
        
        cards.Add(combatState.CreateCard<AdaptiveStrike>(owner));
        cards.Add(combatState.CreateCard<Barrage>(owner));
        cards.Add(combatState.CreateCard<BelieveInYou>(owner));
        cards.Add(combatState.CreateCard<CrashLanding>(owner));
        cards.Add(combatState.CreateCard<Hellraiser>(owner));
        cards.Add(combatState.CreateCard<Orbit>(owner));
        cards.Add(combatState.CreateCard<PerfectedStrike>(owner));
        cards.Add(combatState.CreateCard<MasterPlanner>(owner));
        cards.Add(combatState.CreateCard<SeekingEdge>(owner));
        cards.Add(combatState.CreateCard<DoubleEnergy>(owner));
        cards.Add(combatState.CreateCard<HiddenDaggers>(owner));
        cards.Add(combatState.CreateCard<FanOfKnives>(owner));
        cards.Add(combatState.CreateCard<Shiv>(owner));
        
        return cards;
    }
}