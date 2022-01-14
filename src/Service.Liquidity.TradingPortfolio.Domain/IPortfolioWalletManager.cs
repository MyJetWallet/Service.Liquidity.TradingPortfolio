using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public interface IPortfolioWalletManager
    {
        PortfolioWallet GetInternalWalletByWalletName(string walletName);
        PortfolioWallet GetInternalWalletByWalletId(string walletId);
        Task AddInternalWallet(string walletId, string brokerId, string walletName);
        Task DeleteInternalWalletByWalletName(string walletName);

        PortfolioWallet GetExternalWalletByWalletName(string walletName);
        PortfolioWallet GetExternalWalletByWalletId(string walletId);
        Task AddExternalWallet(string walletName, string brokerId, string sourceName);
        Task DeleteExternalWalletByWalletName(string walletName);

        List<PortfolioWallet> GetWallets();
        PortfolioWallet GetWalletByWalletId(string walletId);
    }
}
