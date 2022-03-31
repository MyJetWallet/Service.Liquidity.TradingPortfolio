using System;
using Service.Liquidity.TradingPortfolio.Grpc.Models;

namespace Service.Liquidity.TradingPortfolio.Cache
{
    public class ManualTradeCacheElement
    {
        public ManualTradeResponse Response { get; set; }
        public DateTime Date;
    }
}