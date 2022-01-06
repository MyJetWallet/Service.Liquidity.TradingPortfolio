using MyJetWallet.Domain.Orders;
using MyJetWallet.Sdk.Service.Tools;
using MyJetWallet.Sdk.ServiceBus;
using Service.FeeShareEngine.Domain.Models.Models;
using Service.IndexPrices.Client;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.PortfolioHedger.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public class PortfolioManager : IPortfolioManager
    {
        private readonly IPortfolioWalletManager _portfolioWalletManager;
        private readonly IServiceBusPublisher<Portfolio> _serviceBusPortfolioPublisher;
        private readonly IServiceBusPublisher<PortfolioFeeShare> _serviceBusFeeSharePublisher;
        private readonly IServiceBusPublisher<PortfolioTrade> _serviceBusTradePublisher;
        private readonly IIndexPricesClient _indexPricesClient;
        private readonly MyLocker _myLocker = new MyLocker();

        private Portfolio _portfolio = new Portfolio() 
        { 
            Assets = new Dictionary<string, Portfolio.Asset>()
        };

        public PortfolioManager(IPortfolioWalletManager portfolioWalletManager,
            IServiceBusPublisher<Portfolio> serviceBusPublisher,
            IIndexPricesClient indexPricesClient,
            IServiceBusPublisher<PortfolioFeeShare> serviceBusFeeSharePublisher, 
            IServiceBusPublisher<PortfolioTrade> serviceBusTradePublisher)
        {
            _portfolioWalletManager = portfolioWalletManager;
            _serviceBusPortfolioPublisher = serviceBusPublisher;
            _indexPricesClient = indexPricesClient;
            _serviceBusFeeSharePublisher = serviceBusFeeSharePublisher;
            _serviceBusTradePublisher = serviceBusTradePublisher;
        }

        public Portfolio GetCurrentPortfolio()
        {
            using var locker = _myLocker.GetLocker().GetAwaiter().GetResult();
            RecalculatePortfolio();
            return _portfolio.MakeCopy();
        }

        public async Task SetDailyVelocityAsync(string assetSymbol, decimal velocity)
        {
            using var locker = await _myLocker.GetLocker();

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
            await _serviceBusPortfolioPublisher.PublishAsync(_portfolio);
        }

        private async Task PublishPortfolioTradesAsync(List<PortfolioTrade> portfolioTrades)
        {
            await _serviceBusTradePublisher.PublishAsync(portfolioTrades);
        }

        private async Task PublishPortfolioFeeShareAsync(PortfolioFeeShare portfolioFeeShare)
        {
            await _serviceBusFeeSharePublisher.PublishAsync(portfolioFeeShare);
        }

        private void ApplySwapItem(string walletId1, string assetId1, decimal volume1,
            string walletId2, string assetId2, decimal volume2)
        {
            var basePortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletId(walletId1);
            if (basePortfolioWallet != null)
            {
                // asset 1
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(assetId1);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(basePortfolioWallet);
                walletBalance1.Balance -= Convert.ToDecimal(volume1);

                // asset 2
                var asset2 = _portfolio.GetOrCreateAssetBySymbol(assetId2);
                var walletBalance2 = asset2.GetOrCreateWalletBalance(basePortfolioWallet);
                walletBalance2.Balance += Convert.ToDecimal(volume2);
            }

            var quotePortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletId(walletId2);
            if (quotePortfolioWallet != null)
            {
                // asset 1
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(assetId1);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(quotePortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume1);

                // asset 2
                var asset2 = _portfolio.GetOrCreateAssetBySymbol(assetId2);
                var walletBalance2 = asset2.GetOrCreateWalletBalance(quotePortfolioWallet);
                walletBalance2.Balance -= Convert.ToDecimal(volume2);
            }
        }

        private void ApplyHadgeTradeItem(string walletId1, string assetId1, decimal volume1,
            string walletId2, string assetId2, decimal volume2)
        {
            var basePortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletId(walletId1);
            if (basePortfolioWallet != null)
            {
                // asset 1
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(assetId1);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(basePortfolioWallet);
                walletBalance1.Balance -= Convert.ToDecimal(volume1);

                // asset 2
                var asset2 = _portfolio.GetOrCreateAssetBySymbol(assetId2);
                var walletBalance2 = asset2.GetOrCreateWalletBalance(basePortfolioWallet);
                walletBalance2.Balance += Convert.ToDecimal(volume2);
            }

            var quotePortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletId(walletId2);
            if (quotePortfolioWallet != null)
            {
                // asset 1
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(assetId1);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(quotePortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume1);

                // asset 2
                var asset2 = _portfolio.GetOrCreateAssetBySymbol(assetId2);
                var walletBalance2 = asset2.GetOrCreateWalletBalance(quotePortfolioWallet);
                walletBalance2.Balance -= Convert.ToDecimal(volume2);
            }
        }

        private void ApplyFeeItem(string walletId, string assetId, decimal volume)
        {
            var basePortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletId(walletId);
            if (basePortfolioWallet != null)
            {
                // asset 1
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(assetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(basePortfolioWallet);
                walletBalance1.Balance -= Convert.ToDecimal(volume);
            }
        }


        public async Task ApplySwapsAsync(IReadOnlyList<SwapMessage> messages)
        {
            using var locker = await _myLocker.GetLocker();
            var portfolioTrades = new List<PortfolioTrade>();

            foreach (var message in messages)
            {
                ApplySwapItem(
                    message.WalletId1, 
                    message.AssetId1, 
                    Convert.ToDecimal(message.Volume1),
                    message.WalletId2, 
                    message.AssetId2, 
                    Convert.ToDecimal(message.Volume2)
                    );

                var (asset1IndexPrice, volume1InUsd) = 
                    _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.AssetId1, Convert.ToDecimal(message.Volume1));

                var (asset2IndexPrice, volume2InUsd) =
                    _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.AssetId2, Convert.ToDecimal(message.Volume2));

                var baseWallet = _portfolioWalletManager.GetInternalWalletByWalletId(message.WalletId1);
                var quoteWallet = _portfolioWalletManager.GetInternalWalletByWalletId(message.WalletId2);

                var portfolioTrade = new PortfolioTrade()
                {
                    TradeId = message.Id,
                    AssociateBrokerId = message.BrokerId,
                    BaseWalletName = message.WalletId1,
                    QuoteWalletName = message.WalletId2,
                    AssociateSymbol = message.AssetId1 + "|" + message.AssetId2,
                    BaseAsset = message.AssetId1,
                    QuoteAsset = message.AssetId2,
                    Side = OrderSide.Sell, //TODO: ???
                    Price = Convert.ToDecimal(message.Volume2) / Convert.ToDecimal(message.Volume1), //TODO: ???
                    BaseVolume = Convert.ToDecimal(message.Volume1),
                    QuoteVolume = Convert.ToDecimal(message.Volume2),
                    BaseVolumeInUsd = volume1InUsd,
                    QuoteVolumeInUsd = volume2InUsd,
                    BaseAssetPriceInUsd = asset1IndexPrice.UsdPrice,
                    QuoteAssetPriceInUsd = asset2IndexPrice.UsdPrice,
                    DateTime = DateTime.UtcNow,
                    Source = baseWallet?.InternalWalletId ?? quoteWallet?.InternalWalletId ?? string.Empty, //TODO: ???
                    Comment = "Swap",//TODO: ???
                    FeeAsset = message.AssetId1,
                    FeeVolume = Convert.ToDecimal(message.Volume1),
                    User = "Converter" //TODO: ???
                };
                portfolioTrades.Add(portfolioTrade);
            }
            await PublishPortfolioTradesAsync(portfolioTrades);
            await PublishPortfolioAsync();
        }

        public async Task ApplyTradesAsync(IReadOnlyList<TradeMessage> messages)
        {
            using var locker = await _myLocker.GetLocker();
            
            var portfolioTrades = new List<PortfolioTrade>();
            foreach (var message in messages)
            {
                ApplyHadgeTradeItem(
                    message.AssociateWalletId,
                    message.BaseAsset,
                    message.Volume,
                    message.AssociateBrokerId,
                    message.QuoteAsset,
                    message.OppositeVolume
                    );

                var (asset1IndexPrice, volume1InUsd) =
                    _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.BaseAsset, Convert.ToDecimal(message.Volume));

                var (asset2IndexPrice, volume2InUsd) =
                    _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.QuoteAsset, Convert.ToDecimal(message.OppositeVolume));

                var baseWallet = _portfolioWalletManager.GetInternalWalletByWalletId(message.AssociateWalletId);
                var quoteWallet = _portfolioWalletManager.GetInternalWalletByWalletId(message.AssociateBrokerId);

                var portfolioTrade = new PortfolioTrade()
                {
                    TradeId = message.Id,
                    AssociateBrokerId = message.AssociateBrokerId,
                    BaseWalletName = message.AssociateWalletId,  //TODO: ???
                    QuoteWalletName = message.AssociateWalletId, //TODO: ???
                    AssociateSymbol = message.BaseAsset + "|" + message.QuoteAsset,
                    BaseAsset = message.BaseAsset,
                    QuoteAsset = message.QuoteAsset,
                    Side = message.Side,
                    Price = message.Price,
                    BaseVolume = Convert.ToDecimal(message.Volume),
                    QuoteVolume = Convert.ToDecimal(message.OppositeVolume),
                    BaseVolumeInUsd = volume1InUsd,
                    QuoteVolumeInUsd = volume2InUsd,
                    BaseAssetPriceInUsd = asset1IndexPrice.UsdPrice,
                    QuoteAssetPriceInUsd = asset2IndexPrice.UsdPrice,
                    DateTime = DateTime.UtcNow,
                    Source = message.Source,
                    Comment = message.Comment,
                    FeeAsset = message.FeeAsset,
                    FeeVolume = Convert.ToDecimal(message.FeeVolume),
                    User = message.User
                };
                portfolioTrades.Add(portfolioTrade);
            }
            await PublishPortfolioTradesAsync(portfolioTrades);
            await PublishPortfolioAsync();
        }

        public async Task ApplyFeeShareAsync(FeeShareEntity message)
        {
            using var locker = await _myLocker.GetLocker();

            ApplyFeeItem(message.ConverterWalletId,
                message.FeeShareAsset,
                message.FeeShareAmountInTargetAsset);

            var portfolioFeeShare = new PortfolioFeeShare()
            {
                OperationId = message.OperationId,
                BrokerId = message.BrokerId,
                WalletFrom = message.ConverterWalletId,
                WalletTo = message.FeeShareWalletId,
                Asset = message.FeeShareAsset,
                Volume = message.FeeShareAmountInTargetAsset,
                Comment = $"FeeShareSettlement:{message.OperationId}",
                ReferrerClientId = message.ReferrerClientId,
                SettlementDate = DateTime.UtcNow,
            };

            await PublishPortfolioFeeShareAsync(portfolioFeeShare);
            await PublishPortfolioAsync();
        }

        public async Task ApplyBalanceAsync(string broker, string wallet, string asset, decimal balance)
        {
            _portfolio.GetOrCreateAssetBySymbol(asset).GetWalletBalanceByPortfolioWalletId(wallet).Balance = balance;
            await PublishPortfolioAsync();
        }
    }
}
