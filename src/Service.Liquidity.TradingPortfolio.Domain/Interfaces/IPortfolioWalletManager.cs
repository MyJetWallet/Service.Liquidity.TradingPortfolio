using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Domain.Interfaces
{
    public interface IPortfolioWalletManager
    {
        PortfolioWallet GetInternalWalletByWalletName(string walletName);
        PortfolioWallet GetInternalById(string walletId);
        Task AddInternalAsync(string walletId, string brokerId, string walletName);
        Task DeleteInternalByNameAsync(string walletName);

        PortfolioWallet GetExternalByName(string walletName);
        PortfolioWallet GetExternalWalletByWalletId(string walletId);
        Task AddExternalAsync(string walletName, string brokerId, string sourceName);
        Task DeleteExternalByNameAsync(string walletName);

        List<PortfolioWallet> Get();
        PortfolioWallet GetById(string walletId);
        PortfolioWallet GetByName(string walletName);
        PortfolioWallet GetByExternalSource(string externalSource);
    }
}
