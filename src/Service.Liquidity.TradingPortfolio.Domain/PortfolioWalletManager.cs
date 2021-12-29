using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Collections.Generic;


namespace Service.Liquidity.TradingPortfolio.Domain
{
    public class PortfolioWalletManager : IPortfolioWalletManager
    {
        private Dictionary<string, PortfolioWallet> _wallets = new Dictionary<string,PortfolioWallet>();

        public void AddExternalWallet(string walletId, string externalSource)
        {
            if (!_wallets.TryGetValue(walletId, out var wallet))
            {
                _wallets[walletId] = new PortfolioWallet()
                {
                    IsInternal = false,
                    ExternalSource = externalSource,
                    Id = null,
                    InternalWalletId = walletId
                };
            }
        }

        public void AddInternalWallet(string walletId, string internalSourceId = "Converter")
        {
            if (!_wallets.TryGetValue(walletId, out var wallet))
            {
                _wallets[walletId] = new PortfolioWallet()
                {
                    IsInternal = true,
                    ExternalSource = null,
                    Id = internalSourceId,
                    InternalWalletId = walletId
                };
            }
            //if (walletId == "SP-Broker")
            //    return new PortfolioWallet()
            //    {
            //        IsInternal = true,
            //        ExternalSource = null,
            //        Id = "Converter",
            //        InternalWalletId = "SP-Broker"
            //    };
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
