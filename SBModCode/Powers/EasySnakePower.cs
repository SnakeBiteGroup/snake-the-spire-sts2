using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;


namespace SBMod.SBModCode.Powers;

public sealed class EasySnakePower : PowerModel
{
    private const string _poisonIncrease = "PoisonIncrease";

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
       new DynamicVar("PoisonIncrease", 1.5m)
    ];
    public override decimal ModifyPowerAmountGiven(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        if (power is PoisonPower && cardSource is Snakebite)
    {
        var easySnake = target?.GetPower<EasySnakePower>();
        if (easySnake != null)
        {
            decimal multiplier = easySnake.DynamicVars[_poisonIncrease].BaseValue;
            return amount * multiplier;
        }
    }
    return amount;
    }
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {

            await PowerCmd.TickDownDuration(this);
       
    }
}
