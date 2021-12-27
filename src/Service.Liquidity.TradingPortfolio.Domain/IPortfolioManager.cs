using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public interface IPortfolioManager
    {
        Task ApplyItemAsync(PortfolioInputModel message);
        Task ApplyItemsAsync(IReadOnlyList<PortfolioInputModel> messages);
        Task SetDailyVelocityAsync(string assetSymbol, decimal velocity);
        Portfolio GetCurrentPortfolio();

    }
}
