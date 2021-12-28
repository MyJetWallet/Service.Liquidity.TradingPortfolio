﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MyJetWallet.Sdk.ServiceBus;
using NUnit.Framework;
using Service.IndexPrices.Client;
using Service.IndexPrices.Domain.Models;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests
{
    public class PortfolioWalletManagerMock : IPortfolioWalletManager
    {
        public Domain.Models.PortfolioWallet GetInternalWalletByWalletId(string walletId)
        {
            if (walletId == "SP-Broker")
                return new Domain.Models.PortfolioWallet()
                {
                    IsInternal = true,
                    ExternalSource = null,
                    Id = "Converter",
                    InternalWalletId = "SP-Broker"
                };

            if (walletId == "SP-Broker-1")
                return new Domain.Models.PortfolioWallet()
                {
                    IsInternal = true,
                    ExternalSource = null,
                    Id = "Converter-1",
                    InternalWalletId = "SP-Broker-1"
                };

            return null;
        }
    }

    public class PortfolioPublisherMock : IServiceBusPublisher<Portfolio>
    {
        public Action<Portfolio> Callback { get; set; }

        public async Task PublishAsync(Portfolio message)
        {
            Callback?.Invoke(message);
        }

        public async Task PublishAsync(IEnumerable<Portfolio> messageList)
        {
            foreach (var message in messageList)
            {
                Callback?.Invoke(message);
            }
        }
    }


    public class PortfolioFeeSharePublisherMock : IServiceBusPublisher<FeeShareSettlement>
    {
        public Action<FeeShareSettlement> Callback { get; set; }

        public async Task PublishAsync(FeeShareSettlement message)
        {
            Callback?.Invoke(message);
        }

        public async Task PublishAsync(IEnumerable<FeeShareSettlement> messageList)
        {
            foreach (var message in messageList)
            {
                Callback?.Invoke(message);
            }
        }
    }

    public class PortfolioTraderPublisherMock : IServiceBusPublisher<PortfolioTrade>
    {
        public Action<PortfolioTrade> Callback { get; set; }

        public async Task PublishAsync(PortfolioTrade message)
        {
            Callback?.Invoke(message);
        }

        public async Task PublishAsync(IEnumerable<PortfolioTrade> messageList)
        {
            foreach (var message in messageList)
            {
                Callback?.Invoke(message);
            }
        }
    }

    public class IndexPricesMock : IIndexPricesClient
    {
        public Dictionary<string, IndexPrice> Prices = new Dictionary<string, IndexPrice>();
        public void Set(string asset, decimal price)
        { 
            if(!Prices.TryGetValue(asset, out var item))
            {
                item = new IndexPrice()
                {
                    Asset = asset,
                    UpdateDate = DateTime.UtcNow,
                };
            }
            item.UsdPrice = price;
            Prices[asset] = item;
        }

        public IndexPrice GetIndexPriceByAssetAsync(string asset)
        {
            if (Prices.TryGetValue(asset, out var item))
            {
                return item;
            }
            return null;
        }

        public (IndexPrice, decimal) GetIndexPriceByAssetVolumeAsync(string asset, decimal volume)
        {
            var item = GetIndexPriceByAssetAsync(asset);

            if (item == null)
                return (new IndexPrice
                {
                    Asset = asset,
                    UpdateDate = DateTime.UtcNow,
                    UsdPrice = 0m
                }, 0m);

            return (item, item.UsdPrice * volume);
        }

        public List<IndexPrice> GetIndexPricesAsync()
        {
            return Prices.Values.ToList();
        }
    }

    public class TestPortfolioManager
    {
        public PortfolioManager _service;
        public PortfolioPublisherMock _portfolioPublisherMock = new PortfolioPublisherMock();
        public IndexPricesMock _indexPricesMock = new IndexPricesMock();
        public PortfolioFeeSharePublisherMock _portfolioFeeSharePublisherMock = new PortfolioFeeSharePublisherMock();
        public PortfolioTraderPublisherMock _portfolioTraderPublisherMock = new PortfolioTraderPublisherMock();

        [SetUp]
        public void Setup()
        {
            _service = new PortfolioManager(new PortfolioWalletManagerMock(),
                _portfolioPublisherMock,
                _indexPricesMock,
                _portfolioFeeSharePublisherMock,
                _portfolioTraderPublisherMock);
        }

        [Test]
        public async Task ApplyClientToBrokerSwap()
        {
            var messageCount = 0;
            _portfolioPublisherMock.Callback = message =>
            {
                messageCount++;
            };

            var swaps = new SwapMessage()
            {
                AccountId1 = "User 1",
                AccountId2 = "Broker",
                AssetId1 = "BTC",
                AssetId2 = "USD",
                BrokerId = "JetWallet",
                Volume1 = "1.0",
                Volume2 = "50000.0",
                WalletId1 = "SP-User 1",
                WalletId2 = "SP-Broker",
                Id = "1",
                MessageId = "1",
                Timestamp = DateTime.Now,
                DifferenceAsset = "USD",
                DifferenceVolumeAbs = 50m,
            };

            await _service.ApplySwapsAsync(new[] { swaps });
            var portfolio = _service.GetCurrentPortfolio();

            portfolio.Assets["BTC"].WalletBalances["Converter"].Balance.Should().Be(1m);
            portfolio.Assets["USD"].WalletBalances["Converter"].Balance.Should().Be(-50000m);
            messageCount.Should().Be(1);
        }

        [Test]
        public async Task ApplyClientToClientSwap()
        {
            var swaps = new SwapMessage()
            {
                AccountId1 = "User 1",
                AccountId2 = "User 2",
                AssetId1 = "BTC",
                AssetId2 = "USD",
                BrokerId = "JetWallet",
                Volume1 = "1.0",
                Volume2 = "50000.0",
                WalletId1 = "User 1",
                WalletId2 = "User 2",
                Id = "1",
                MessageId = "1",
                Timestamp = DateTime.Now,
                DifferenceAsset = "USD",
                DifferenceVolumeAbs = 50m,
            };

            await _service.ApplySwapsAsync(new[] { swaps });
            var portfolio = _service.GetCurrentPortfolio();

            portfolio.GetOrCreateAssetBySymbol("BTC").GetWalletBalanceByPortfolioWalletId("Converter").Should().BeNull();
            portfolio.GetOrCreateAssetBySymbol("USD").GetWalletBalanceByPortfolioWalletId("Converter").Should().BeNull();
        }

        [Test]
        public async Task ApplyBrokerToClientSwap()
        {
            var swaps = new SwapMessage()
            {
                AccountId1 = "Broker",
                AccountId2 = "User 1",
                AssetId1 = "BTC",
                AssetId2 = "USD",
                BrokerId = "JetWallet",
                Volume1 = "1.0",
                Volume2 = "50000.0",
                WalletId1 = "SP-Broker",
                WalletId2 = "SP-User 1",
                Id = "1",
                MessageId = "1",
                Timestamp = DateTime.Now,
                DifferenceAsset = "USD",
                DifferenceVolumeAbs = 50m,
            };

            await _service.ApplySwapsAsync(new[] { swaps });
            var portfolio = _service.GetCurrentPortfolio();

            portfolio.Assets["BTC"].WalletBalances["Converter"].Balance.Should().Be(-1m);
            portfolio.Assets["USD"].WalletBalances["Converter"].Balance.Should().Be(50000m);
        }


        [Test]
        public async Task ApplySeveralSwaps()
        {
            await _service.ApplySwapsAsync(new[]
            {
                ClientToBroker(1m, "BTC", 50000m, "USD"),
                ClientToBroker(1m, "ETH", 4000m, "USD"),
                ClientToBroker(9000m, "USD", 2m, "ETH"),
            });

            var portfolio = _service.GetCurrentPortfolio();

            portfolio.Assets["BTC"].WalletBalances["Converter"].Balance.Should().Be(1m);
            portfolio.Assets["USD"].WalletBalances["Converter"].Balance.Should().Be(-45000m);
            portfolio.Assets["ETH"].WalletBalances["Converter"].Balance.Should().Be(-1m);
        }

        [Test]
        public async Task ApplySeveralSwapsZero()
        {
            await _service.ApplySwapsAsync(new[]
            {
                ClientToBroker(1m, "BTC", 40000m, "USD"),
                ClientToBroker(40000m, "USD", 10m, "ETH"),
            });

            var portfolio = _service.GetCurrentPortfolio();

            portfolio.Assets["BTC"].WalletBalances["Converter"].Balance.Should().Be(1m);
            portfolio.Assets["USD"].WalletBalances["Converter"].Balance.Should().Be(0m);
            portfolio.Assets["ETH"].WalletBalances["Converter"].Balance.Should().Be(-10m);
        }

        private SwapMessage ClientToBroker(decimal vol1, string asset1, decimal vol2, string asset2, string brokerWallet = "SP-Broker")
        {
            return new SwapMessage()
            {
                AccountId1 = "User 1",
                AccountId2 = "Broker",
                AssetId1 = asset1,
                AssetId2 = asset2,
                BrokerId = "JetWallet",
                Volume1 = vol1.ToString(),
                Volume2 = vol2.ToString(),
                WalletId1 = "SP-User 1",
                WalletId2 = brokerWallet,
                Id = "1",
                MessageId = "1",
                Timestamp = DateTime.Now,
                DifferenceAsset = "USD",
                DifferenceVolumeAbs = 50m,
            };
        }

        [Test]
        public async Task SetVelocity()
        {
            await _service.SetDailyVelocityAsync("BTC", 0.02m);
            await _service.SetDailyVelocityAsync("ETH", 0.05m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").DailyVelocity.Should().Be(0.02m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").DailyVelocity.Should().Be(0.05m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").DailyVelocity.Should().Be(0.00m);
        }

        [Test]
        public async Task CheckPortfolioInUsd()
        {
            _indexPricesMock.Set("BTC", 41000m);
            _indexPricesMock.Set("USD", 1m);
            _indexPricesMock.Set("ETH", 4200m);

            await _service.ApplySwapsAsync(new[]
            {
                ClientToBroker(1m, "BTC", 40000m, "USD", "SP-Broker"),
                ClientToBroker(40000m, "USD", 10m, "ETH", "SP-Broker-1"),
            });

            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").GetWalletBalanceByPortfolioWalletId("Converter").Balance.Should().Be(1m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").GetWalletBalanceByPortfolioWalletId("Converter").BalanceInUsd.Should().Be(41000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").GetWalletBalanceByPortfolioWalletId("Converter").Should().BeNull();
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").GetWalletBalanceByPortfolioWalletId("Converter").Balance.Should().Be(-40000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").GetWalletBalanceByPortfolioWalletId("Converter").BalanceInUsd.Should().Be(-40000m);

            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").GetWalletBalanceByPortfolioWalletId("Converter-1").Should().BeNull();
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").GetWalletBalanceByPortfolioWalletId("Converter-1").Balance.Should().Be(-10m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").GetWalletBalanceByPortfolioWalletId("Converter-1").BalanceInUsd.Should().Be(-42000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").GetWalletBalanceByPortfolioWalletId("Converter-1").Balance.Should().Be(40000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").GetWalletBalanceByPortfolioWalletId("Converter-1").BalanceInUsd.Should().Be(40000m);

            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").NetBalance.Should().Be(1m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").NetBalanceInUsd.Should().Be(41000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").NetBalance.Should().Be(-10m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").NetBalanceInUsd.Should().Be(-42000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").NetBalance.Should().Be(0m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").NetBalanceInUsd.Should().Be(0m);

            _service.GetCurrentPortfolio().TotalNetInUsd.Should().Be(-1000);
        }

        [Test]
        public async Task CheckPortfolioVelocityNet()
        {
            _indexPricesMock.Set("BTC", 41000m);
            _indexPricesMock.Set("USD", 1m);
            _indexPricesMock.Set("ETH", 4200m);

            await _service.ApplySwapsAsync(new[]
            {
                ClientToBroker(1m, "BTC", 40000m, "USD", "SP-Broker"),
                ClientToBroker(40000m, "USD", 10m, "ETH", "SP-Broker-1"),
            });

            await _service.SetDailyVelocityAsync("BTC", 0.01m);
            await _service.SetDailyVelocityAsync("ETH", 0.02m);

            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").NetBalance.Should().Be(1m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").NetBalanceInUsd.Should().Be(41000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").DailyVelocity.Should().Be(0.01m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").DailyVelocityRiskInUsd.Should().Be(-410m);

            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").NetBalance.Should().Be(-10m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").NetBalanceInUsd.Should().Be(-42000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").DailyVelocity.Should().Be(0.02m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").DailyVelocityRiskInUsd.Should().Be(-840m);

            _service.GetCurrentPortfolio().TotalDailyVelocityRiskInUsd.Should().Be(-1250m);
        }
    }
}