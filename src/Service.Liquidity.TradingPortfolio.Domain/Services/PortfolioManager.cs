using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Orders;
using MyJetWallet.Sdk.Service.Tools;
using MyNoSqlServer.Abstractions;
using Service.AssetsDictionary.Client;
using Service.FeeShareEngine.Domain.Models.Models;
using Service.IndexPrices.Client;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.Hedger.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Interfaces;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;
using Service.Liquidity.TradingPortfolio.Domain.Utils;

namespace Service.Liquidity.TradingPortfolio.Domain.Services
{
    public class PortfolioManager : IPortfolioManager
    {
        private readonly IPortfolioWalletManager _portfolioWalletManager;
        private readonly IIndexAssetDictionaryClient _indexAssetDictionaryClient;
        private readonly ILogger<PortfolioManager> _logger;
        private readonly IEventsPublisher _eventsPublisher;
        private readonly IMyNoSqlServerDataWriter<PortfolioNoSql> _myNoSqlPortfolioWriter;
        private readonly IIndexPricesClient _indexPricesClient;
        private readonly MyLocker _myLocker = new();

        private Portfolio _cachedPortfolio = new()
        {
            Assets = new Dictionary<string, Portfolio.Asset>()
        };

        public PortfolioManager(IPortfolioWalletManager portfolioWalletManager,
            IIndexPricesClient indexPricesClient,
            IMyNoSqlServerDataWriter<PortfolioNoSql> myNoSqlPortfolioWriter,
            IIndexAssetDictionaryClient indexAssetDictionaryClient,
            ILogger<PortfolioManager> logger,
            IEventsPublisher eventsPublisher
        )
        {
            _portfolioWalletManager = portfolioWalletManager;
            _indexPricesClient = indexPricesClient;
            _myNoSqlPortfolioWriter = myNoSqlPortfolioWriter;
            _indexAssetDictionaryClient = indexAssetDictionaryClient;
            _logger = logger;
            _eventsPublisher = eventsPublisher;
        }

        public void Load()
        {
            var data = _myNoSqlPortfolioWriter.GetAsync().GetAwaiter().GetResult().FirstOrDefault();

            using var locker = _myLocker.GetLocker().GetAwaiter().GetResult();

            if (data == null)
            {
                return;
            }

            _cachedPortfolio = data.Portfolio;
            CalculatePortfolio();
        }

        public Portfolio GetCurrentPortfolio()
        {
            using var locker = _myLocker.GetLocker().GetAwaiter().GetResult();

            CalculatePortfolio();
            var portfolio = _cachedPortfolio.MakeCopy();
            portfolio.Assets ??= new();

            return portfolio;
        }

        private void CalculatePortfolio()
        {
            if (_cachedPortfolio.Assets == null)
            {
                return;
            }

            var totalNetInUsd = 0m;
            var totalDailyVelocityRiskInUsd = 0m;
            var totalNegativeNetInUsd = 0m;
            var totalPositiveNetInUsd = 0m;
            var totalInternalBalanceInUsd = 0m;
            var totalExternalBalanceInUsd = 0m;

            foreach (var asset in _cachedPortfolio?.Assets?.Values)
            {
                var netBalance = 0m;
                var netBalanceInUsd = 0m;
                var netInternalBalance = 0m;
                var netInternalBalanceInUsd = 0m;
                var netExternalBalance = 0m;
                var netExternalBalanceInUsd = 0m;

                foreach (var walletBalance in asset.WalletBalances.Values)
                {
                    var (_, usdBalance) =
                        _indexPricesClient.GetIndexPriceByAssetVolumeAsync(asset.Symbol, walletBalance.Balance);
                    walletBalance.BalanceInUsd = MoneyTools.To2Digits(usdBalance);
                    netBalance += walletBalance.Balance;
                    netBalanceInUsd = MoneyTools.To2Digits(walletBalance.BalanceInUsd + netBalanceInUsd);

                    if (walletBalance.Wallet.IsInternal)
                    {
                        netInternalBalance += walletBalance.Balance;
                        netInternalBalanceInUsd =
                            MoneyTools.To2Digits(netInternalBalanceInUsd + walletBalance.BalanceInUsd);
                    }
                    else
                    {
                        netExternalBalance += walletBalance.Balance;
                        netExternalBalanceInUsd =
                            MoneyTools.To2Digits(netExternalBalanceInUsd + walletBalance.BalanceInUsd);
                    }
                }

                asset.NetBalance = netBalance;
                asset.NetBalanceInUsd = netBalanceInUsd;
                asset.NetInternalBalance = netInternalBalance;
                asset.NetInternalBalanceInUsd = netInternalBalanceInUsd;
                asset.NetExternalBalance = netExternalBalance;
                asset.NetExternalBalanceInUsd = netExternalBalanceInUsd;

                asset.DailyVelocity = MoneyTools.To2Digits(asset.NetBalance >= 0
                    ? asset.DailyVelocityLowOpen
                    : asset.DailyVelocityHighOpen);
                asset.DailyVelocityRiskInUsd =
                    MoneyTools.To2Digits(-Math.Abs(netBalanceInUsd * asset.DailyVelocity) / 100);

                totalNetInUsd = MoneyTools.To2Digits(totalNetInUsd + asset.NetBalanceInUsd);
                totalDailyVelocityRiskInUsd =
                    MoneyTools.To2Digits(totalDailyVelocityRiskInUsd + asset.DailyVelocityRiskInUsd);

                totalNegativeNetInUsd = MoneyTools.To2Digits(totalNegativeNetInUsd + asset.GetNegativeNetInUsd());
                totalPositiveNetInUsd = MoneyTools.To2Digits(totalPositiveNetInUsd + asset.GetPositiveNetInUsd());
                totalInternalBalanceInUsd = MoneyTools.To2Digits(totalInternalBalanceInUsd + netInternalBalanceInUsd);
                totalExternalBalanceInUsd = MoneyTools.To2Digits(totalExternalBalanceInUsd + netExternalBalanceInUsd);
            }

            _cachedPortfolio.TotalNetInUsd = totalNetInUsd;
            _cachedPortfolio.TotalDailyVelocityRiskInUsd = totalDailyVelocityRiskInUsd;
            _cachedPortfolio.TotalNegativeNetInUsd = totalNegativeNetInUsd;
            _cachedPortfolio.TotalNegativeNetPercent =
                MoneyTools.To2Digits(totalNegativeNetInUsd != 0m ? totalNetInUsd / totalNegativeNetInUsd * 100m : 0m);
            _cachedPortfolio.TotalPositiveNetInUsd = totalPositiveNetInUsd;
            _cachedPortfolio.TotalPositiveNetInPercent =
                MoneyTools.To2Digits(totalPositiveNetInUsd != 0m ? totalNetInUsd / totalPositiveNetInUsd * 100m : 0m);
            _cachedPortfolio.TotalLeverage = totalNetInUsd != 0m ? totalPositiveNetInUsd / totalNetInUsd : 0m;
            _cachedPortfolio.InternalBalanceInUsd = totalInternalBalanceInUsd;
            _cachedPortfolio.ExternalBalanceInUsd = totalExternalBalanceInUsd;
        }

        private async Task RecalculateAndSaveAndPublishPortfolioAsync()
        {
            CalculatePortfolio();
            await _myNoSqlPortfolioWriter.InsertOrReplaceAsync(PortfolioNoSql.Create(_cachedPortfolio));
            await _eventsPublisher.PublishAsync(_cachedPortfolio);
        }

        private bool ApplySwapItem(string walletId1, string assetId1, decimal volume1,
            string walletId2, string assetId2, decimal volume2, string brokerId)
        {
            var applied = false;

            var basePortfolioWallet = _portfolioWalletManager.GetInternalById(walletId1);

            if (basePortfolioWallet != null)
            {
                ApplyChangeBalance(assetId1, basePortfolioWallet, -Convert.ToDecimal(volume1));
                ApplyChangeBalance(assetId2, basePortfolioWallet, Convert.ToDecimal(volume2));
                applied = true;
            }

            var quotePortfolioWallet = _portfolioWalletManager.GetInternalById(walletId2);

            if (quotePortfolioWallet != null)
            {
                ApplyChangeBalance(assetId1, quotePortfolioWallet, Convert.ToDecimal(volume1));
                ApplyChangeBalance(assetId2, quotePortfolioWallet, -Convert.ToDecimal(volume2));
                applied = true;
            }

            return applied;
        }

        private void ApplyChangeBalance(string asset, PortfolioWallet portfolioWallet, decimal volume)
        {
            var index = _indexAssetDictionaryClient.GetIndexAsset(DomainConstants.DefaultBroker, asset);

            if (index == null)
            {
                var portfolioAsset = _cachedPortfolio.GetOrCreateAssetBySymbol(asset);
                var walletBalance = portfolioAsset.GetOrCreateWalletBalance(portfolioWallet);
                walletBalance.Balance += Convert.ToDecimal(volume);
                return;
            }

            foreach (var item in index.Basket)
            {
                var basketAsset = item.Symbol;
                var basketVolume = item.Volume * volume;

                var portfolioAsset = _cachedPortfolio.GetOrCreateAssetBySymbol(basketAsset);
                var walletBalance = portfolioAsset.GetOrCreateWalletBalance(portfolioWallet);
                walletBalance.Balance += Convert.ToDecimal(basketVolume);
            }
        }

        private bool ApplyFee(string walletId, string assetId, decimal volume, string brokerId)
        {
            var applied = false;
            var basePortfolioWallet = _portfolioWalletManager.GetInternalById(walletId);

            if (basePortfolioWallet != null)
            {
                var asset1 = _cachedPortfolio.GetOrCreateAssetBySymbol(assetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(basePortfolioWallet);
                walletBalance1.Balance -= Convert.ToDecimal(volume);
                applied = true;
            }

            return applied;
        }

        private bool ApplySettlement(string assetId, string walletId1, decimal volume1,
            string walletId2, decimal volume2)
        {
            var applied = false;
            var basePortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletName(walletId1);

            if (basePortfolioWallet != null)
            {
                var asset1 = _cachedPortfolio.GetOrCreateAssetBySymbol(assetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(basePortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume1);
                applied = true;
            }

            var quotePortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletName(walletId2);

            if (quotePortfolioWallet != null)
            {
                var asset1 = _cachedPortfolio.GetOrCreateAssetBySymbol(assetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(quotePortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume2);
                applied = true;
            }

            var baseExPortfolioWallet = _portfolioWalletManager.GetExternalByName(walletId1);

            if (baseExPortfolioWallet != null)
            {
                var asset1 = _cachedPortfolio.GetOrCreateAssetBySymbol(assetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(baseExPortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume1);
                applied = true;
            }

            var quoteExPortfolioWallet = _portfolioWalletManager.GetExternalByName(walletId2);

            if (quoteExPortfolioWallet != null)
            {
                var asset1 = _cachedPortfolio.GetOrCreateAssetBySymbol(assetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(quoteExPortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume2);
                applied = true;
            }

            return applied;
        }

        private bool ApplyTrade(string walletId, string assetId1, decimal volume1,
            string assetId2, decimal volume2,
            string feeAssetId, decimal feeVolume)
        {
            var applied = false;
            var internalPortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletName(walletId);

            if (internalPortfolioWallet != null)
            {
                var asset1 = _cachedPortfolio.GetOrCreateAssetBySymbol(assetId1);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(internalPortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume1);

                var asset2 = _cachedPortfolio.GetOrCreateAssetBySymbol(assetId2);
                var walletBalance2 = asset2.GetOrCreateWalletBalance(internalPortfolioWallet);
                walletBalance2.Balance += Convert.ToDecimal(volume2);

                var feeAsset = _cachedPortfolio.GetOrCreateAssetBySymbol(feeAssetId);
                var feeWalletBalance = feeAsset.GetOrCreateWalletBalance(internalPortfolioWallet);
                feeWalletBalance.Balance += Convert.ToDecimal(feeVolume);
                applied = true;
            }

            var externalPortfolioWallet = _portfolioWalletManager.GetExternalByName(walletId);

            if (externalPortfolioWallet != null)
            {
                var asset1 = _cachedPortfolio.GetOrCreateAssetBySymbol(assetId1);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(externalPortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume1);

                var asset2 = _cachedPortfolio.GetOrCreateAssetBySymbol(assetId2);
                var walletBalance2 = asset2.GetOrCreateWalletBalance(externalPortfolioWallet);
                walletBalance2.Balance += Convert.ToDecimal(volume2);

                var feeAsset = _cachedPortfolio.GetOrCreateAssetBySymbol(feeAssetId);
                var feeWalletBalance = feeAsset.GetOrCreateWalletBalance(externalPortfolioWallet);
                feeWalletBalance.Balance += Convert.ToDecimal(feeVolume);
                applied = true;
            }

            return applied;
        }

        public async Task ApplySwapsAsync(IReadOnlyList<SwapMessage> messages)
        {
            using var locker = await _myLocker.GetLocker();
            var portfolioTrades = new List<PortfolioTrade>();

            foreach (var message in messages)
            {
                if (!ApplySwapItem(
                        message.WalletId1,
                        message.AssetId1,
                        Convert.ToDecimal(message.Volume1),
                        message.WalletId2,
                        message.AssetId2,
                        Convert.ToDecimal(message.Volume2),
                        message.BrokerId
                    ))
                {
                    continue;
                }

                var (asset1IndexPrice, volume1InUsd) =
                    _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.AssetId1,
                        Convert.ToDecimal(message.Volume1));

                var (asset2IndexPrice, volume2InUsd) =
                    _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.AssetId2,
                        Convert.ToDecimal(message.Volume2));

                var (feeAssetIndexPrice, feeVolumeInUsd) =
                    _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.FeeAsset,
                        Convert.ToDecimal(message.FeeAmount));
                
                var baseWallet = _portfolioWalletManager.GetInternalById(message.WalletId1);
                var quoteWallet = _portfolioWalletManager.GetInternalById(message.WalletId2);

                var portfolioTrade = new PortfolioTrade
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
                    Source = baseWallet?.Name ?? quoteWallet?.Name ?? string.Empty, //TODO: ???
                    Comment = "Swap", //TODO: ???
                    FeeAsset = message.FeeAsset,
                    FeeVolume = Convert.ToDecimal(message.FeeAmount),
                    FeeVolumeInUsd = feeVolumeInUsd,
                    FeeAssetPriceInUsd = feeAssetIndexPrice.UsdPrice,
                    Type = PortfolioTradeType.Swap,
                    //User = message //TODO: ???
                };
                portfolioTrades.Add(portfolioTrade);
            }

            await _eventsPublisher.PublishAsync(portfolioTrades);
            await RecalculateAndSaveAndPublishPortfolioAsync();
        }

        public async Task ApplyTradeAsync(TradeMessage message)
        {
            using var locker = await _myLocker.GetLocker();

            var portfolioTrades = new List<PortfolioTrade>();
            var applayTrade = false;
            if (message.Side == OrderSide.Sell)
            {
                applayTrade = ApplyTrade(
                    message.AssociateWalletId,
                    message.BaseAsset,
                    -Math.Abs(message.Volume),
                    message.QuoteAsset,
                    Math.Abs(message.OppositeVolume),
                    message.FeeAsset,
                    -Math.Abs(message.FeeVolume));
            }
            else
            {
                applayTrade = ApplyTrade(
                    message.AssociateWalletId,
                    message.BaseAsset,
                    Math.Abs(message.Volume),
                    message.QuoteAsset,
                    -Math.Abs(message.OppositeVolume),
                    message.FeeAsset,
                    -Math.Abs(message.FeeVolume));
            }

            if (!applayTrade)
            {
                return;
            }

            var (baseAssetIndexPrice, baseVolumeInUsd) =
                _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.BaseAsset,
                    Convert.ToDecimal(message.Volume));
            var (quoteAssetIndexPrice, quoteVolumeInUsd) =
                _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.QuoteAsset,
                    Convert.ToDecimal(message.OppositeVolume));

            var (feeAssetIndexPrice, feeVolumeInUsd) =
                _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.FeeAsset,
                    Convert.ToDecimal(message.FeeVolume));
            
            var portfolioTrade = new PortfolioTrade
            {
                TradeId = message.Id,
                AssociateBrokerId = message.AssociateBrokerId,
                BaseWalletName = message.AssociateWalletId,
                QuoteWalletName = message.AssociateWalletId,
                AssociateSymbol = message.BaseAsset + "|" + message.QuoteAsset,
                BaseAsset = message.BaseAsset,
                QuoteAsset = message.QuoteAsset,
                Side = message.Side,
                Price = message.Price,
                BaseVolume = Convert.ToDecimal(message.Volume),
                QuoteVolume = Convert.ToDecimal(message.OppositeVolume),
                BaseVolumeInUsd = baseVolumeInUsd,
                QuoteVolumeInUsd = quoteVolumeInUsd,
                BaseAssetPriceInUsd = baseAssetIndexPrice.UsdPrice,
                QuoteAssetPriceInUsd = quoteAssetIndexPrice.UsdPrice,
                DateTime = DateTime.UtcNow,
                Source = message.Source,
                Comment = message.Comment,
                FeeAsset = message.FeeAsset,
                FeeVolume = Convert.ToDecimal(message.FeeVolume),
                FeeVolumeInUsd = feeVolumeInUsd,
                FeeAssetPriceInUsd = feeAssetIndexPrice.UsdPrice,
                Type = message.Type,
                User = message.User
            };
            portfolioTrades.Add(portfolioTrade);

            await _eventsPublisher.PublishAsync(portfolioTrades);
            await RecalculateAndSaveAndPublishPortfolioAsync();
        }


        public async Task ApplyFeeShareAsync(FeeShareEntity message)
        {
            using var locker = await _myLocker.GetLocker();

            if (!ApplyFee(message.ConverterWalletId,
                    message.FeeShareAsset,
                    message.FeeShareAmountInTargetAsset,
                    message.BrokerId))
            {
                return;
            }

            var portfolioFeeShare = new PortfolioFeeShare
            {
                OperationId = message.OperationId,
                BrokerId = message.BrokerId,
                WalletFrom = message.ConverterWalletId,
                WalletTo = message.FeeShareWalletId,
                Asset = message.FeeShareAsset,
                VolumeFrom = message.FeeShareAmountInTargetAsset,
                VolumeTo = message.FeeShareAmountInTargetAsset,
                Comment = $"FeeShareSettlement:{message.OperationId}",
                ReferrerClientId = message.ReferrerClientId,
                SettlementDate = DateTime.UtcNow,
            };

            await _eventsPublisher.PublishAsync(portfolioFeeShare);
            await RecalculateAndSaveAndPublishPortfolioAsync();
        }

        public async Task SetManualBalanceAsync(string wallet, string asset, decimal balance,
            string comment, string user)
        {
            using var locker = await _myLocker.GetLocker();

            var portfolioAsset = _cachedPortfolio.GetOrCreateAssetBySymbol(asset);
            var portfolioWallet = _portfolioWalletManager.GetByName(wallet);
            if (portfolioWallet == null)
            {
                throw new Exception($"Can't find portfolio wallet: {wallet}");
            }

            var walletBalance = portfolioAsset.GetOrCreateWalletBalance(portfolioWallet);
            var oldBalance = walletBalance.Balance;
            walletBalance.Balance = balance;

            var portfolioChangeBalance = new PortfolioChangeBalance
            {
                BrokerId = portfolioWallet.BrokerId,
                WalletName = wallet,
                Asset = asset,
                Balance = walletBalance.Balance,
                UpdateDate = DateTime.UtcNow,
                Comment = comment,
                User = user,
                BalanceBeforeUpdate = oldBalance
            };

            await _eventsPublisher.PublishAsync(portfolioChangeBalance);
            await RecalculateAndSaveAndPublishPortfolioAsync();
        }

        public async Task SetVelocityLowHighAsync(string asset, decimal lowOpen, decimal highOpen)
        {
            using var locker = await _myLocker.GetLocker();

            var portfolioAsset = _cachedPortfolio.GetAssetBySymbol(asset);
            if (portfolioAsset != null)
            {
                portfolioAsset.DailyVelocityLowOpen = lowOpen;
                portfolioAsset.DailyVelocityHighOpen = highOpen;
            }

            await RecalculateAndSaveAndPublishPortfolioAsync();
        }

        public async Task SetManualSettlementAsync(PortfolioSettlement settlement)
        {
            if (!ApplySettlement(
                    settlement.Asset,
                    settlement.WalletFrom,
                    settlement.VolumeFrom,
                    settlement.WalletTo,
                    settlement.VolumeTo
                ))
            {
                return;
            }

            await _eventsPublisher.PublishAsync(settlement);
            await RecalculateAndSaveAndPublishPortfolioAsync();
        }

        public async Task ApplyHedgeOperationAsync(HedgeOperation operation)
        {
            using var locker = await _myLocker.GetLocker();
            var portfolioTrades = new List<PortfolioTrade>();

            foreach (var hedgeTrade in operation.HedgeTrades ?? new List<HedgeTrade>())
            {
                var wallet = _portfolioWalletManager.GetByExternalSource(hedgeTrade.ExchangeName);

                if (wallet == null)
                {
                    _logger.LogWarning("HedgeTrade can't be applied. Wallet not found {@trade}", hedgeTrade);
                    return;
                }
                
                var baseAsset = _cachedPortfolio.GetOrCreateAssetBySymbol(hedgeTrade.BaseAsset);
                var baseWalletBalance = baseAsset.GetOrCreateWalletBalance(wallet);
                var quoteAsset = _cachedPortfolio.GetOrCreateAssetBySymbol(hedgeTrade.QuoteAsset);
                var quoteWalletBalance = quoteAsset.GetOrCreateWalletBalance(wallet);
                
                if (hedgeTrade.Side == OrderSide.Buy)
                {
                    baseWalletBalance.Increase(hedgeTrade.BaseVolume);
                    quoteWalletBalance.Decrease(hedgeTrade.QuoteVolume);
                }
                else if (hedgeTrade.Side == OrderSide.Sell)
                {
                    baseWalletBalance.Decrease(hedgeTrade.BaseVolume);
                    quoteWalletBalance.Increase(hedgeTrade.QuoteVolume);
                }
                else
                {
                    throw new Exception($"Can't apply hedge trade. Not supported order side {hedgeTrade.Side.ToString()}");
                }

                if (hedgeTrade.FeeVolume > 0 && !string.IsNullOrEmpty(hedgeTrade.FeeAsset))
                {
                    var feeAsset = _cachedPortfolio.GetOrCreateAssetBySymbol(hedgeTrade.FeeAsset);
                    var feeWalletBalance = feeAsset.GetOrCreateWalletBalance(wallet);
                    feeWalletBalance.Decrease(hedgeTrade.FeeVolume);
                }

                var (baseIndexPrice, baseVolumeInUsd) = _indexPricesClient
                    .GetIndexPriceByAssetVolumeAsync(hedgeTrade.BaseAsset, hedgeTrade.BaseVolume);
                var (quoteIndexPrice, quoteVolumeInUsd) = _indexPricesClient
                    .GetIndexPriceByAssetVolumeAsync(hedgeTrade.QuoteAsset, hedgeTrade.QuoteVolume);
                var (feeIndexPrice, feeVolumeInUsd) = _indexPricesClient
                    .GetIndexPriceByAssetVolumeAsync(hedgeTrade.FeeAsset, hedgeTrade.FeeVolume);
                
                var portfolioTrade = hedgeTrade.ToPortfolioTrade(wallet.Name,
                    baseVolumeInUsd, baseIndexPrice.UsdPrice,
                    quoteVolumeInUsd, quoteIndexPrice.UsdPrice,
                    feeVolumeInUsd, feeIndexPrice.UsdPrice);
                portfolioTrades.Add(portfolioTrade);
            }

            _cachedPortfolio.HedgeOperationId = operation.Id;

            await _eventsPublisher.PublishAsync(portfolioTrades);
            await RecalculateAndSaveAndPublishPortfolioAsync();
        }
    }
}