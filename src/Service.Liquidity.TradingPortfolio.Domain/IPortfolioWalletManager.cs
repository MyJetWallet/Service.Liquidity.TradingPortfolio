using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public interface IPortfolioWalletManager
    {
        PortfolioWallet GetInternalWalletByWalletId(string walletId);
        void AddInternalWallet(string walletId, string internalWalletId);
        PortfolioWallet GetExternalWalletByWalletId(string walletId);
        void AddExternalWallet(string walletId, string internalWalletId);
    }
}
