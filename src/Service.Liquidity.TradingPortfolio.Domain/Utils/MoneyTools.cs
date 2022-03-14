using System;

namespace Service.Liquidity.TradingPortfolio.Domain.Utils;

public static class MoneyTools
{
    /// <summary>
    /// Money Round 0.004->0.00, 0.005->0.01 
    /// </summary>
    public static Decimal To2Digits(Decimal money)
    {
        var flag = money < 0;
        money = flag ? money * -1 : money;
        money = Math.Round(money, 2);
        return flag ? money * -1 : money;
    }
}