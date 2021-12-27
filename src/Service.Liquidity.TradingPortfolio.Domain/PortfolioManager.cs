using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public class PortfolioManager : IPortfolioManager
    {
        private readonly IPortfolioWalletManager _portfolioWalletManager;
        private Portfolio _portfolio = new Portfolio() 
        { 
            Assets = new Dictionary<string, Portfolio.Asset>()
        };

        public PortfolioManager(IPortfolioWalletManager portfolioWalletManager)
        {
            _portfolioWalletManager = portfolioWalletManager;

        }
        public async Task ApplySwaps(IReadOnlyList<SwapMessage> messages)
        {
            foreach (var message in messages) 
            {
                ApplyMessage(message);
            }
        }

        public Portfolio GetCurrentPortfolio()
        {
            return _portfolio;
        }

        private void ApplyMessage(SwapMessage message)
        {
            var portfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletId(message.WalletId2);
            if(portfolioWallet != null)
            {
                // asset 1
                var asset1 = _portfolio.GetAssetBySymbol(message.AssetId1);
                var walletBalance1 = asset1.GetOrCreate(portfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(message.Volume1);

                // asset 2
                var asset2 = _portfolio.GetAssetBySymbol(message.AssetId2);
                var walletBalance2 = asset2.GetOrCreate(portfolioWallet);
                walletBalance2.Balance -= Convert.ToDecimal(message.Volume2);
            }

            portfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletId(message.WalletId1);
            if (portfolioWallet != null)
            {
                // asset 1
                var asset1 = _portfolio.GetAssetBySymbol(message.AssetId1);
                var walletBalance1 = asset1.GetOrCreate(portfolioWallet);
                walletBalance1.Balance -= Convert.ToDecimal(message.Volume1);

                // asset 2
                var asset2 = _portfolio.GetAssetBySymbol(message.AssetId2);
                var walletBalance2 = asset2.GetOrCreate(portfolioWallet);
                walletBalance2.Balance += Convert.ToDecimal(message.Volume2);
            }
        }
    }
}
