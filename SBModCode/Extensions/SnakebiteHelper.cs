
using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.Extensions;

public static class SnakebiteHelper
{
    private static readonly HashSet<Type> _snakebiteTypes = new();
    
    public static void RegisterSnakebiteCard<T>() where T : CardModel
    {
        _snakebiteTypes.Add(typeof(T));
    }
    
    public static bool IsSnakebiteCard(CardModel card)
    {
        if (card == null) return false;
        return _snakebiteTypes.Contains(card.GetType()) || card is Snakebite;
    }
    
    public static int CountSnakebiteCards(IEnumerable<CardModel> cards)
    {
        if (cards == null) return 0;
        int count = 0;
        foreach (var card in cards)
        {
            if (IsSnakebiteCard(card)) count++;
        }
        return count;
    }
    
    public static int CountHandSnakebites(Player player)
    {
        if (player == null) return 0;
        var handPile = PileType.Hand.GetPile(player);
        return CountSnakebiteCards(handPile.Cards);
    }
    
    public static int CountDrawPileSnakebites(Player player)
    {
        if (player == null) return 0;
        var drawPile = PileType.Draw.GetPile(player);
        return CountSnakebiteCards(drawPile.Cards);
    }
    
    public static int CountDiscardPileSnakebites(Player player)
    {
        if (player == null) return 0;
        var discardPile = PileType.Discard.GetPile(player);
        return CountSnakebiteCards(discardPile.Cards);
    }
    
    public static int CountExhaustPileSnakebites(Player player)
    {
        if (player == null) return 0;
        var exhaustPile = PileType.Exhaust.GetPile(player);
        return CountSnakebiteCards(exhaustPile.Cards);
    }
    
    public static int CountAllSnakebites(Player player)
    {
        if (player == null) return 0;
        return CountHandSnakebites(player) + 
               CountDrawPileSnakebites(player) + 
               CountDiscardPileSnakebites(player) + 
               CountExhaustPileSnakebites(player);
    }
}