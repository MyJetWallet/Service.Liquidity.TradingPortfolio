using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public interface IPortfolioManager
    {
        Task ApplySwapsAsync(IReadOnlyList<Liquidity.Converter.Domain.Models.SwapMessage> messages);
        Portfolio GetCurrentPortfolio();
        Task SetDailyVelocityAsync(string assetSymbol, decimal velocity);
    }
}
