using MyJetWallet.Sdk.ServiceBus;
using Service.IndexPrices.Client;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public class PortfolioManager : IPortfolioManager
    {
        private readonly IPortfolioWalletManager _portfolioWalletManager;
        private readonly IServiceBusPublisher<Portfolio> _serviceBusPublisher;
        private readonly IIndexPricesClient _indexPricesClient;
        private Portfolio _portfolio = new Portfolio() 
        { 
            Assets = new Dictionary<string, Portfolio.Asset>()
        };

        public PortfolioManager(IPortfolioWalletManager portfolioWalletManager, 
            IServiceBusPublisher<Portfolio> serviceBusPublisher,
            IIndexPricesClient indexPricesClient)
        {
            _portfolioWalletManager = portfolioWalletManager;
            _serviceBusPublisher = serviceBusPublisher;
            _indexPricesClient = indexPricesClient;
        }

        public Portfolio GetCurrentPortfolio()
        {
            RecalculatePortfolio();
            return _portfolio;
        }

        public async Task SetDailyVelocityAsync(string assetSymbol, decimal velocity)
        {
            _portfolio.GetOrCreateAssetBySymbol(assetSymbol).DailyVelocity = velocity;
            await PublishPortfolioAsync();
        }

        private void RecalculatePortfolio()
        {
            var totalNetInUsd = 0m;
            var totalDailyVelocityRiskInUsd = 0m;
            foreach (var asset in _portfolio.Assets.Values)
            {
                var netBalance = 0m;
                var netBalanceInUsd = 0m;
                foreach (var walletBalance in asset.WalletBalances.Values)
                {
                    var (_, usdBalance) = _indexPricesClient.GetIndexPriceByAssetVolumeAsync(asset.Symbol, walletBalance.Balance);
                    walletBalance.BalanceInUsd = usdBalance;
                    netBalance += walletBalance.Balance;
                    netBalanceInUsd += usdBalance;
                }
                asset.NetBalance = netBalance;
                asset.NetBalanceInUsd = netBalanceInUsd;
                asset.DailyVelocityRiskInUsd = - Math.Abs(netBalanceInUsd * asset.DailyVelocity);
                
                totalNetInUsd += asset.NetBalanceInUsd;
                totalDailyVelocityRiskInUsd += asset.DailyVelocityRiskInUsd;
            }
            _portfolio.TotalNetInUsd = totalNetInUsd;
            _portfolio.TotalDailyVelocityRiskInUsd = totalDailyVelocityRiskInUsd;
        }

        private async Task PublishPortfolioAsync()
        {
            RecalculatePortfolio();
            await _serviceBusPublisher.PublishAsync(_portfolio);
        }

        public async Task ApplyItemAsync(PortfolioInputModel message)
        {
            ApplyItem(message);
            await PublishPortfolioAsync();
        }
        public async Task ApplyItemsAsync(IReadOnlyList<PortfolioInputModel> messages)
        {
            foreach (var message in messages)
            {
                ApplyItem(message);
            }
            await PublishPortfolioAsync();
        }

        private void ApplyItem(PortfolioInputModel message)
        {
            var portfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletId(message.To.WalletId);
            if (portfolioWallet != null)
            {
                // asset 1
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(message.From.AssetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(portfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(message.From.Volume);

                // asset 2
                var asset2 = _portfolio.GetOrCreateAssetBySymbol(message.To.AssetId);
                var walletBalance2 = asset2.GetOrCreateWalletBalance(portfolioWallet);
                walletBalance2.Balance -= Convert.ToDecimal(message.To.Volume);
            }

            portfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletId(message.From.WalletId);
            if (portfolioWallet != null)
            {
                // asset 1
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(message.From.AssetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(portfolioWallet);
                walletBalance1.Balance -= Convert.ToDecimal(message.From.Volume);

                // asset 2
                var asset2 = _portfolio.GetOrCreateAssetBySymbol(message.To.AssetId);
                var walletBalance2 = asset2.GetOrCreateWalletBalance(portfolioWallet);
                walletBalance2.Balance += Convert.ToDecimal(message.To.Volume);
            }
        }
    }
}
