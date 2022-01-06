using Service.FeeShareEngine.Domain.Models.Models;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.PortfolioHedger.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public interface IPortfolioManager
    {
        Task ApplySwapsAsync(IReadOnlyList<SwapMessage> messages);
        Task ApplyTradesAsync(IReadOnlyList<TradeMessage> messages);
        Task ApplyFeeShareAsync(FeeShareEntity message);
        Task SetDailyVelocityAsync(string assetSymbol, decimal velocity);
        Portfolio GetCurrentPortfolio();
        Task ApplyBalanceAsync(string broker, string wallet, string asset, decimal balance);
    }
}
