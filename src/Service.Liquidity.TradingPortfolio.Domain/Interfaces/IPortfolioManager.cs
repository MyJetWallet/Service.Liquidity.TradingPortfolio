using System.Collections.Generic;
using System.Threading.Tasks;
using Service.FeeShareEngine.Domain.Models.Models;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.Hedger.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Domain.Interfaces
{
    public interface IPortfolioManager
    {
        Task ApplySwapsAsync(IReadOnlyList<SwapMessage> messages);
        Task ApplyTradeAsync(TradeMessage messages);
        Task ApplyFeeShareAsync(FeeShareEntity message);
        Task SetVelocityLowHighAsync(string asset, decimal lowOpen, decimal highOpen);
        Portfolio GetCurrentPortfolio();
        Task SetManualBalanceAsync(string wallet, string asset, decimal balance, string comment, string user);
        Task SetManualSettlementAsync(PortfolioSettlement settlement);
        Task ApplyHedgeOperationAsync(HedgeOperation operation);
    }
}