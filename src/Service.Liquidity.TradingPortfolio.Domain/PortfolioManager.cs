using MyJetWallet.Domain.Orders;
using MyJetWallet.Sdk.Service.Tools;
using MyJetWallet.Sdk.ServiceBus;
using Service.FeeShareEngine.Domain.Models.Models;
using Service.IndexPrices.Client;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyNoSqlServer.Abstractions;
using Service.AssetsDictionary.Client;
using Service.Liquidity.Hedger.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;
using Service.Liquidity.TradingPortfolio.Domain.Utils;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public class PortfolioManager : IPortfolioManager
    {
        private readonly IPortfolioWalletManager _portfolioWalletManager;
        private readonly IServiceBusPublisher<Portfolio> _serviceBusPortfolioPublisher;
        private readonly IServiceBusPublisher<PortfolioFeeShare> _serviceBusFeeSharePublisher;
        private readonly IServiceBusPublisher<PortfolioTrade> _serviceBusTradePublisher;
        private readonly IServiceBusPublisher<PortfolioSettlement> _serviceBusSettementPublisher;
        private readonly IServiceBusPublisher<PortfolioChangeBalance> _serviceBusChangeBalancePublisher;
        private readonly IIndexAssetDictionaryClient _indexAssetDictionaryClient;
        private readonly ILogger<PortfolioManager> _logger;
        private readonly IMyNoSqlServerDataWriter<PortfolioNoSql> _myNoSqlPortfolioWriter;
        private readonly IIndexPricesClient _indexPricesClient;
        private readonly MyLocker _myLocker = new();

        private Portfolio _portfolio = new()
        {
            Assets = new Dictionary<string, Portfolio.Asset>()
        };


        public PortfolioManager(IPortfolioWalletManager portfolioWalletManager,
            IServiceBusPublisher<Portfolio> serviceBusPublisher,
            IIndexPricesClient indexPricesClient,
            IServiceBusPublisher<PortfolioFeeShare> serviceBusFeeSharePublisher,
            IServiceBusPublisher<PortfolioTrade> serviceBusTradePublisher,
            IServiceBusPublisher<PortfolioSettlement> serviceBusSettementPublisher,
            IMyNoSqlServerDataWriter<PortfolioNoSql> myNoSqlPortfolioWriter,
            IServiceBusPublisher<PortfolioChangeBalance> serviceBusChangeBalancePublisher,
            IIndexAssetDictionaryClient indexAssetDictionaryClient,
            ILogger<PortfolioManager> logger
        )
        {
            _portfolioWalletManager = portfolioWalletManager;
            _serviceBusPortfolioPublisher = serviceBusPublisher;
            _indexPricesClient = indexPricesClient;
            _serviceBusFeeSharePublisher = serviceBusFeeSharePublisher;
            _serviceBusTradePublisher = serviceBusTradePublisher;
            _serviceBusSettementPublisher = serviceBusSettementPublisher;
            _myNoSqlPortfolioWriter = myNoSqlPortfolioWriter;
            _serviceBusChangeBalancePublisher = serviceBusChangeBalancePublisher;
            _indexAssetDictionaryClient = indexAssetDictionaryClient;
            _logger = logger;
        }

        public void Load()
        {
            var data = _myNoSqlPortfolioWriter.GetAsync().GetAwaiter().GetResult().FirstOrDefault();

            using var locker = _myLocker.GetLocker().GetAwaiter().GetResult();

            if (data == null)
            {
                return;
            }

            _portfolio = data.Portfolio;
            RecalculatePortfolio();
        }

        public Portfolio GetCurrentPortfolio()
        {
            using var locker = _myLocker.GetLocker().GetAwaiter().GetResult();

            RecalculatePortfolio();
            var portfolio = _portfolio.MakeCopy();
            portfolio.Assets ??= new();

            return portfolio;
        }

        private void RecalculatePortfolio()
        {
            if (_portfolio.Assets == null)
            {
                return;
            }

            var totalNetInUsd = 0m;
            var totalDailyVelocityRiskInUsd = 0m;
            var totalNegativeNetInUsd = 0m;
            var totalPositiveNetInUsd = 0m;
            var totalInternalBalanceInUsd = 0m;
            var totalExternalBalanceInUsd = 0m;

            foreach (var asset in _portfolio?.Assets?.Values)
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

            _portfolio.TotalNetInUsd = totalNetInUsd;
            _portfolio.TotalDailyVelocityRiskInUsd = totalDailyVelocityRiskInUsd;
            _portfolio.TotalNegativeNetInUsd = totalNegativeNetInUsd;
            _portfolio.TotalNegativeNetPercent =
                MoneyTools.To2Digits(totalNegativeNetInUsd != 0m ? totalNetInUsd / totalNegativeNetInUsd * 100m : 0m);
            _portfolio.TotalPositiveNetInUsd = totalPositiveNetInUsd;
            _portfolio.TotalPositiveNetInPercent =
                MoneyTools.To2Digits(totalPositiveNetInUsd != 0m ? totalNetInUsd / totalPositiveNetInUsd * 100m : 0m);
            _portfolio.TotalLeverage = totalNetInUsd != 0m ? totalPositiveNetInUsd / totalNetInUsd : 0m;
            _portfolio.InternalBalanceInUsd = totalInternalBalanceInUsd;
            _portfolio.ExternalBalanceInUsd = totalExternalBalanceInUsd;
        }

        private async Task PublishPortfolioAsync()
        {
            RecalculatePortfolio();
            await _myNoSqlPortfolioWriter.InsertOrReplaceAsync(PortfolioNoSql.Create(_portfolio));
            await _serviceBusPortfolioPublisher.PublishAsync(_portfolio);
        }

        private async Task PublishTradesAsync(List<PortfolioTrade> portfolioTrades)
        {
            await _serviceBusTradePublisher.PublishAsync(portfolioTrades);
        }

        private async Task PublishPortfolioFeeShareAsync(PortfolioFeeShare portfolioFeeShare)
        {
            await _serviceBusFeeSharePublisher.PublishAsync(portfolioFeeShare);
        }

        private bool ApplySwapItem(string walletId1, string assetId1, decimal volume1,
            string walletId2, string assetId2, decimal volume2, string brokerId)
        {
            var retval = false;
            var basePortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletId(walletId1);

            if (basePortfolioWallet != null)
            {
                ApplyChangeBalance(assetId1, basePortfolioWallet, -Convert.ToDecimal(volume1));
                ApplyChangeBalance(assetId2, basePortfolioWallet, Convert.ToDecimal(volume2));
                retval = true;
            }

            var quotePortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletId(walletId2);

            if (quotePortfolioWallet != null)
            {
                ApplyChangeBalance(assetId1, quotePortfolioWallet, Convert.ToDecimal(volume1));
                ApplyChangeBalance(assetId2, quotePortfolioWallet, -Convert.ToDecimal(volume2));
                retval = true;
            }

            return retval;
        }

        private void ApplyChangeBalance(string asset, PortfolioWallet portfolioWallet, decimal volume)
        {
            var index = _indexAssetDictionaryClient.GetIndexAsset(DomainConstants.DefaultBroker, asset);
            if (index == null)
            {
                var portfolioAsset = _portfolio.GetOrCreateAssetBySymbol(asset);
                var walletBalance = portfolioAsset.GetOrCreateWalletBalance(portfolioWallet);
                walletBalance.Balance += Convert.ToDecimal(volume);
                return;
            }

            foreach (var item in index.Basket)
            {
                var basketAsset = item.Symbol;
                var basketVolume = item.Volume * volume;

                var portfolioAsset = _portfolio.GetOrCreateAssetBySymbol(basketAsset);
                var walletBalance = portfolioAsset.GetOrCreateWalletBalance(portfolioWallet);
                walletBalance.Balance += Convert.ToDecimal(basketVolume);
            }
        }

        private bool ApplyFeeItem(string walletId, string assetId, decimal volume, string brokerId)
        {
            var retval = false;
            var basePortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletId(walletId);
            if (basePortfolioWallet != null)
            {
                // asset 1
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(assetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(basePortfolioWallet);
                walletBalance1.Balance -= Convert.ToDecimal(volume);
                retval = true;
            }

            return retval;
        }

        private bool ApplySettlementItem(string assetId, string walletId1, decimal volume1,
            string walletId2, decimal volume2)
        {
            var retval = false;
            var basePortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletName(walletId1);
            if (basePortfolioWallet != null)
            {
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(assetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(basePortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume1);
                retval = true;
            }

            var quotePortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletName(walletId2);
            if (quotePortfolioWallet != null)
            {
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(assetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(quotePortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume2);
                retval = true;
            }

            var baseExPortfolioWallet = _portfolioWalletManager.GetExternalWalletByWalletName(walletId1);
            if (baseExPortfolioWallet != null)
            {
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(assetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(baseExPortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume1);
                retval = true;
            }

            var quoteExPortfolioWallet = _portfolioWalletManager.GetExternalWalletByWalletName(walletId2);
            if (quoteExPortfolioWallet != null)
            {
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(assetId);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(quoteExPortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume2);
                retval = true;
            }

            return retval;
        }

        private bool ApplyTradeItem(string walletId, string assetId1, decimal volume1,
            string assetId2, decimal volume2,
            string feeAssetId, decimal feeVolume)
        {
            var retval = false;
            var internalPortfolioWallet = _portfolioWalletManager.GetInternalWalletByWalletName(walletId);
            if (internalPortfolioWallet != null)
            {
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(assetId1);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(internalPortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume1);

                var asset2 = _portfolio.GetOrCreateAssetBySymbol(assetId2);
                var walletBalance2 = asset2.GetOrCreateWalletBalance(internalPortfolioWallet);
                walletBalance2.Balance += Convert.ToDecimal(volume2);

                var feeAsset = _portfolio.GetOrCreateAssetBySymbol(feeAssetId);
                var feeWalletBalance = feeAsset.GetOrCreateWalletBalance(internalPortfolioWallet);
                feeWalletBalance.Balance += Convert.ToDecimal(feeVolume);
                retval = true;
            }

            var externalPortfolioWallet = _portfolioWalletManager.GetExternalWalletByWalletName(walletId);
            if (externalPortfolioWallet != null)
            {
                var asset1 = _portfolio.GetOrCreateAssetBySymbol(assetId1);
                var walletBalance1 = asset1.GetOrCreateWalletBalance(externalPortfolioWallet);
                walletBalance1.Balance += Convert.ToDecimal(volume1);

                var asset2 = _portfolio.GetOrCreateAssetBySymbol(assetId2);
                var walletBalance2 = asset2.GetOrCreateWalletBalance(externalPortfolioWallet);
                walletBalance2.Balance += Convert.ToDecimal(volume2);

                var feeAsset = _portfolio.GetOrCreateAssetBySymbol(feeAssetId);
                var feeWalletBalance = feeAsset.GetOrCreateWalletBalance(externalPortfolioWallet);
                feeWalletBalance.Balance += Convert.ToDecimal(feeVolume);
                retval = true;
            }

            return retval;
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

                var baseWallet = _portfolioWalletManager.GetInternalWalletByWalletId(message.WalletId1);
                var quoteWallet = _portfolioWalletManager.GetInternalWalletByWalletId(message.WalletId2);

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
                    FeeAsset = message.AssetId1,
                    FeeVolume = Convert.ToDecimal(message.Volume1),
                    //User = message //TODO: ???
                };
                portfolioTrades.Add(portfolioTrade);
            }

            await PublishTradesAsync(portfolioTrades);
            await PublishPortfolioAsync();
        }

        public async Task ApplyTradesAsync(IReadOnlyList<TradeMessage> messages)
        {
            using var locker = await _myLocker.GetLocker();

            var portfolioTrades = new List<PortfolioTrade>();
            foreach (var message in messages)
            {
                if (!ApplyTradeItem(
                        message.AssociateWalletId,
                        message.BaseAsset,
                        message.Volume,
                        message.QuoteAsset,
                        message.OppositeVolume,
                        message.FeeAsset,
                        message.FeeVolume))
                {
                    continue;
                }

                var (asset1IndexPrice, volume1InUsd) =
                    _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.BaseAsset,
                        Convert.ToDecimal(message.Volume));
                var (asset2IndexPrice, volume2InUsd) =
                    _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.QuoteAsset,
                        Convert.ToDecimal(message.OppositeVolume));

                var portfolioTrade = new PortfolioTrade()
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

            await PublishTradesAsync(portfolioTrades);
            await PublishPortfolioAsync();
        }

        public async Task ApplyTradeAsync(TradeMessage message)
        {
            using var locker = await _myLocker.GetLocker();

            var portfolioTrades = new List<PortfolioTrade>();
            if (!ApplyTradeItem(
                    message.AssociateWalletId,
                    message.BaseAsset,
                    message.Volume,
                    message.QuoteAsset,
                    message.OppositeVolume,
                    message.FeeAsset,
                    message.FeeVolume))
            {
                return;
            }

            var (asset1IndexPrice, volume1InUsd) =
                _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.BaseAsset,
                    Convert.ToDecimal(message.Volume));
            var (asset2IndexPrice, volume2InUsd) =
                _indexPricesClient.GetIndexPriceByAssetVolumeAsync(message.QuoteAsset,
                    Convert.ToDecimal(message.OppositeVolume));

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

            await PublishTradesAsync(portfolioTrades);
            await PublishPortfolioAsync();
        }


        public async Task ApplyFeeShareAsync(FeeShareEntity message)
        {
            using var locker = await _myLocker.GetLocker();

            if (!ApplyFeeItem(message.ConverterWalletId,
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

            await PublishPortfolioFeeShareAsync(portfolioFeeShare);
            await PublishPortfolioAsync();
        }

        public async Task SetManualBalanceAsync(string wallet, string asset, decimal balance,
            string comment, string user)
        {
            using var locker = await _myLocker.GetLocker();

            var portfolioAsset = _portfolio.GetOrCreateAssetBySymbol(asset);
            var portfolioWallet = _portfolioWalletManager.GetWalletByWalletName(wallet);
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

            await PublishPortfolioChangeBalanceAsync(portfolioChangeBalance);
            await PublishPortfolioAsync();
        }

        private async Task PublishPortfolioChangeBalanceAsync(PortfolioChangeBalance portfolioChangeBalance)
        {
            await _serviceBusChangeBalancePublisher.PublishAsync(portfolioChangeBalance);
        }

        public async Task SetDailyVelocityAsync(string asset, decimal velocity)
        {
            using var locker = await _myLocker.GetLocker();

            var portfolioAsset = _portfolio.GetAssetBySymbol(asset);
            if (portfolioAsset != null)
            {
                portfolioAsset.DailyVelocityLowOpen = velocity;
                portfolioAsset.DailyVelocityHighOpen = velocity;
            }

            await PublishPortfolioAsync();
        }

        public async Task SetVelocityLowHighAsync(string asset, decimal lowOpen, decimal highOpen)
        {
            using var locker = await _myLocker.GetLocker();

            var portfolioAsset = _portfolio.GetAssetBySymbol(asset);
            if (portfolioAsset != null)
            {
                portfolioAsset.DailyVelocityLowOpen = lowOpen;
                portfolioAsset.DailyVelocityHighOpen = highOpen;
            }

            await PublishPortfolioAsync();
        }

        public async Task SetManualSettlementAsync(PortfolioSettlement settlement)
        {
            if (!ApplySettlementItem(
                    settlement.Asset,
                    settlement.WalletFrom,
                    settlement.VolumeFrom,
                    settlement.WalletTo,
                    settlement.VolumeTo
                ))
            {
                return;
            }

            await PublishSettlementAsync(settlement);
            await PublishPortfolioAsync();
        }

        public async Task ApplyHedgeOperationAsync(HedgeOperation operation)
        {
            using var locker = await _myLocker.GetLocker();

            var portfolioTrades = new List<PortfolioTrade>();
            foreach (var trade in operation.Trades)
            {
                var wallet = _portfolioWalletManager.GetWalletByExternalSource(trade.ExchangeName);

                if (wallet == null)
                {
                    _logger.LogWarning("HedgeTrade can't be applied. Wallet not found {@trade}", trade);
                    return;
                }

                var baseAsset = _portfolio.GetOrCreateAssetBySymbol(trade.BaseAsset);
                var baseWalletBalance = baseAsset.GetOrCreateWalletBalance(wallet);
                baseWalletBalance.Balance += Convert.ToDecimal(trade.BaseVolume);

                var quoteAsset = _portfolio.GetOrCreateAssetBySymbol(trade.QuoteAsset);
                var quoteWalletBalance = quoteAsset.GetOrCreateWalletBalance(wallet);
                quoteWalletBalance.Balance += Convert.ToDecimal(trade.QuoteVolume);

                var (baseIndexPrice, baseVolumeInUsd) =
                    _indexPricesClient.GetIndexPriceByAssetVolumeAsync(trade.BaseAsset,
                        Convert.ToDecimal(trade.BaseVolume));
                var (quoteIndexPrice, quoteVolumeInUsd) =
                    _indexPricesClient.GetIndexPriceByAssetVolumeAsync(trade.QuoteAsset,
                        Convert.ToDecimal(trade.QuoteVolume));

                portfolioTrades.Add(new()
                {
                    TradeId = trade.Id,
                    AssociateBrokerId = "jetwallet",
                    BaseWalletName = wallet.Name,
                    QuoteWalletName = wallet.Name,
                    AssociateSymbol = trade.BaseAsset + "|" + trade.QuoteAsset,
                    BaseAsset = trade.BaseAsset,
                    QuoteAsset = trade.QuoteAsset,
                    Side = OrderSide.Buy,
                    Price = trade.Price,
                    BaseVolume = Convert.ToDecimal(trade.BaseVolume),
                    QuoteVolume = Convert.ToDecimal(trade.QuoteVolume),
                    BaseVolumeInUsd = baseVolumeInUsd,
                    QuoteVolumeInUsd = quoteVolumeInUsd,
                    BaseAssetPriceInUsd = baseIndexPrice.UsdPrice,
                    QuoteAssetPriceInUsd = quoteIndexPrice.UsdPrice,
                    DateTime = DateTime.UtcNow,
                    Source = "Hedger",
                    Comment = "Hedge trade",
                    FeeAsset = "",
                    FeeVolume = 0,
                    User = ""
                });
            }

            _portfolio.HedgeOperationId = operation.Id;

            await PublishTradesAsync(portfolioTrades);
            await PublishPortfolioAsync();
        }

        private async Task PublishSettlementAsync(PortfolioSettlement settlement)
        {
            await _serviceBusSettementPublisher.PublishAsync(settlement);
        }
    }
}