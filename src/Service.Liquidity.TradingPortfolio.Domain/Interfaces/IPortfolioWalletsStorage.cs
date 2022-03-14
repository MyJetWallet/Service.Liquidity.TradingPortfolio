using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Domain.Interfaces;

public interface IPortfolioWalletsStorage
{
    Task AddOrUpdateAsync(PortfolioWallet model);
    Task<IEnumerable<PortfolioWallet>> GetAsync();
    Task<PortfolioWallet> GetAsync(string id);
    Task DeleteAsync(string id);
    Task BulkInsetOrUpdateAsync(IEnumerable<PortfolioWallet> models);
}