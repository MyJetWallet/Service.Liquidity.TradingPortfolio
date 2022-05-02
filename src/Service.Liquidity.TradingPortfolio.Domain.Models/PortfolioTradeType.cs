using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Domain.Models;

public enum PortfolioTradeType
{
    Unknown = 0,
    Swap,
    Manual,
    Hedge
}