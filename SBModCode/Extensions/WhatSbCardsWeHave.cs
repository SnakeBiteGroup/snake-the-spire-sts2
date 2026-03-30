using MegaCrit.Sts2.Core.Models.Cards;

namespace SBMod.SBModCode.Extensions;

public static class WhatSbCardsWeHave
{
    public static void RegisterAll()
    {
        SnakebiteHelper.RegisterSnakebiteCard<Snakebite>();
        // 继续注册其他蛇咬牌
    }
}