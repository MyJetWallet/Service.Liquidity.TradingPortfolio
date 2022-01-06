using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Collections.Generic;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public interface IPortfolioWalletManager
    {
        PortfolioWallet GetInternalWalletByWalletId(string walletId);
        void AddInternalWallet(string walletId, string brokerId, string walletName);
        PortfolioWallet GetExternalWalletByWalletId(string walletId);
        void AddExternalWallet(string walletId, string brokerId, string sourceName);
        List<PortfolioWallet> GetWallets();
    }
}
