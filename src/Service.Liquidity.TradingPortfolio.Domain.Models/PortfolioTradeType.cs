using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Domain.Models;

[Flags] public enum PortfolioTradeType
{
    None = 0,
    Swap = 1,
    Manual = 2,
    AutoHedge = 4
}