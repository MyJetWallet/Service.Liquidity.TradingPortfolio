using System;
using Service.FeeShareEngine.Domain.Models.Models;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.Hedger.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public interface IPortfolioManager
    {
        Task ApplySwapsAsync(IReadOnlyList<SwapMessage> messages);
        Task ApplyTradesAsync(IReadOnlyList<TradeMessage> messages);
        Task ApplyTradeAsync(TradeMessage messages);
        Task ApplyFeeShareAsync(FeeShareEntity message);
        [Obsolete("SetManualVelocityAsync is obsolete, use SetManualVelocityLowHighAsync", false)]
        Task SetDailyVelocityAsync(string assetSymbol, decimal velocity);
        Task SetVelocityLowHighAsync(string asset, decimal lowOpen, decimal highOpen);
        Portfolio GetCurrentPortfolio();
        Task SetManualBalanceAsync(string wallet, string asset, decimal balance, string comment, string user);
        Task SetManualSettelmentAsync(PortfolioSettlement settlement);
        Task ApplyHedgeOperationAsync(HedgeOperation operation);
    }
}
