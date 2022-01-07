using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public interface IPortfolioWalletManager
    {
        PortfolioWallet GetInternalWalletByWalletId(string walletId);
        Task AddInternalWallet(string walletId, string brokerId, string walletName);
        PortfolioWallet GetExternalWalletByWalletId(string walletId);
        Task AddExternalWallet(string walletName, string brokerId, string sourceName);
        List<PortfolioWallet> GetWallets();
        PortfolioWallet GetWalleteByWalletId(string walletId);
    }
}
