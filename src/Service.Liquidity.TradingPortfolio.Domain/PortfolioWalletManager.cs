using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Collections.Generic;


namespace Service.Liquidity.TradingPortfolio.Domain
{
    public class PortfolioWalletManager : IPortfolioWalletManager
    {
        private Dictionary<string, PortfolioWallet> _wallets = new Dictionary<string,PortfolioWallet>();

        public void AddExternalWallet(string walletId, string brokerId, string sourceName)
        {
            if (!_wallets.TryGetValue(walletId, out var wallet))
            {
                _wallets[walletId] = new PortfolioWallet()
                {
                    IsInternal = false,
                    ExternalSource = sourceName,
                    Id = null,
                    InternalWalletId = walletId,
                    BrokerId = brokerId
                };
            }
        }

        public void AddInternalWallet(string walletId, string brokerId, string walletName = "Converter")
        {
            if (!_wallets.TryGetValue(walletId, out var wallet))
            {
                _wallets[walletId] = new PortfolioWallet()
                {
                    IsInternal = true,
                    ExternalSource = null,
                    Id = walletName,
                    InternalWalletId = walletId,
                    BrokerId = brokerId
                };
            }
        }

        public PortfolioWallet GetExternalWalletByWalletId(string walletId)
        {
            if (!_wallets.TryGetValue(walletId, out var wallet))
            {
                return null;
            }

            return wallet.IsInternal == false ? wallet : null;
        }

        public PortfolioWallet GetInternalWalletByWalletId(string walletId)
        {
            if(!_wallets.TryGetValue(walletId, out var wallet))
            {
                return null;    
            }
            
            return wallet.IsInternal ==  true ? wallet : null;
        }
    }
}
