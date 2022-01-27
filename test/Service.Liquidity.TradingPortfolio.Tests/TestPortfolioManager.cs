﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Service.AssetsDictionary.Domain.Models;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain;

namespace Service.Liquidity.TradingPortfolio.Tests
{
    public class TestPortfolioManager
    {
        public PortfolioManager _service;
        public PortfolioPublisherMock _portfolioPublisherMock;
        public IndexPricesMock _indexPricesMock;
        public PortfolioFeeSharePublisherMock _portfolioFeeSharePublisherMock;
        public PortfolioTraderPublisherMock _portfolioTraderPublisherMock;
        private PortfolioManualSettlementPublisherMock _portfolioManualSettelmentMock;
        private PortfolioMyNoSqlWriterMock _myNoSqlPortfolioWriter;
        public PortfolioChangeBalancePublisherMock _portfolioChangeBalancePublisherMock;
        public IndexAssetDictionaryClientMock _indexAssetDictionaryClientMock;

        [SetUp]
        public void Setup()
        {

            _portfolioPublisherMock = new PortfolioPublisherMock();
            _indexPricesMock = new IndexPricesMock();
            _portfolioFeeSharePublisherMock = new PortfolioFeeSharePublisherMock();
            _portfolioTraderPublisherMock = new PortfolioTraderPublisherMock();
            _portfolioManualSettelmentMock = new PortfolioManualSettlementPublisherMock();
            _myNoSqlPortfolioWriter = new PortfolioMyNoSqlWriterMock();
            _portfolioChangeBalancePublisherMock = new PortfolioChangeBalancePublisherMock();
            _indexAssetDictionaryClientMock = new IndexAssetDictionaryClientMock();

            _service = new PortfolioManager(new PortfolioWalletManagerMock(),
                    _portfolioPublisherMock,
                    _indexPricesMock,
                    _portfolioFeeSharePublisherMock,
                    _portfolioTraderPublisherMock,
                    _portfolioManualSettelmentMock,
                    _myNoSqlPortfolioWriter,
                    _portfolioChangeBalancePublisherMock,
                    _indexAssetDictionaryClientMock
                    );
            _service.Load();
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
                WalletId1 = "SP-User 1",
                WalletId2 = "SP-User 2",
                Id = "1",
                MessageId = "1",
                Timestamp = DateTime.Now,
                DifferenceAsset = "USD",
                DifferenceVolumeAbs = 50m,
            };

            await _service.ApplySwapsAsync(new[] { swaps });
            var portfolio = _service.GetCurrentPortfolio();

            portfolio.GetOrCreateAssetBySymbol("BTC").GetWalletBalanceByPortfolioWalletName("Converter").Should().BeNull();
            portfolio.GetOrCreateAssetBySymbol("USD").GetWalletBalanceByPortfolioWalletName("Converter").Should().BeNull();
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
            await _service.ApplySwapsAsync(new[]
            {
                ClientToBroker(1m, "BTC", 40000m, "USD", "SP-Broker"),
                ClientToBroker(40000m, "USD", 10m, "ETH", "SP-Broker-1"),
            });
            
            await _service.SetVelocityLowHighAsync("BTC", 0.01m, 0.1m);
            await _service.SetVelocityLowHighAsync("ETH", 0.02m, 0.2m);
            await _service.SetVelocityLowHighAsync("USD", 0.03m, 0.3m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").DailyVelocityLowOpen.Should().Be(0.01m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").DailyVelocityHighOpen.Should().Be(0.1m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").DailyVelocityLowOpen.Should().Be(0.02m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").DailyVelocityHighOpen.Should().Be(0.2m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").DailyVelocityLowOpen.Should().Be(0.03m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").DailyVelocityHighOpen.Should().Be(0.3m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("EUR").DailyVelocityLowOpen.Should().Be(0m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("EUR").DailyVelocityHighOpen.Should().Be(0m);

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

            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").GetWalletBalanceByPortfolioWalletName("Converter").Balance.Should().Be(1m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").GetWalletBalanceByPortfolioWalletName("Converter").BalanceInUsd.Should().Be(41000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").GetWalletBalanceByPortfolioWalletName("Converter").Should().BeNull();
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").GetWalletBalanceByPortfolioWalletName("Converter").Balance.Should().Be(-40000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").GetWalletBalanceByPortfolioWalletName("Converter").BalanceInUsd.Should().Be(-40000m);

            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").GetWalletBalanceByPortfolioWalletName("Converter-1").Should().BeNull();
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").GetWalletBalanceByPortfolioWalletName("Converter-1").Balance.Should().Be(-10m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").GetWalletBalanceByPortfolioWalletName("Converter-1").BalanceInUsd.Should().Be(-42000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").GetWalletBalanceByPortfolioWalletName("Converter-1").Balance.Should().Be(40000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").GetWalletBalanceByPortfolioWalletName("Converter-1").BalanceInUsd.Should().Be(40000m);

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

            await _service.SetVelocityLowHighAsync("BTC", 0.01m, 0.1m);
            await _service.SetVelocityLowHighAsync("ETH", 0.02m, 0.2m);

            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").NetBalance.Should().Be(1m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").NetBalanceInUsd.Should().Be(41000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").DailyVelocityLowOpen.Should().Be(0.01m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").DailyVelocityHighOpen.Should().Be(0.1m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").DailyVelocityRiskInUsd.Should().Be(-41m);

            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").NetBalance.Should().Be(-10m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").NetBalanceInUsd.Should().Be(-42000m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").DailyVelocityLowOpen.Should().Be(0.02m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").DailyVelocityHighOpen.Should().Be(0.2m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").DailyVelocityRiskInUsd.Should().Be(-8.4m);

            _service.GetCurrentPortfolio().TotalDailyVelocityRiskInUsd.Should().Be(-49.4m);
        }

        [Test]
        public async Task ApplySwapWithIndexInstrument_1()
        {
            _indexAssetDictionaryClientMock.Data.Add("CIC", new IndexAsset
            {
                Symbol = "CIC",
                Broker = "jetwallet",
                Basket = new List<IndexAsset.BasketAsset>()
                    {
                        new IndexAsset.BasketAsset
                        {
                            Symbol = "BTC",
                            Volume = 1,
                            PriceInstrumentSymbol = "BTCUSD",
                            DirectInstrumentPrice = true,
                            TargetRebalanceWeight = 0.5m
                        },
                        new IndexAsset.BasketAsset
                        {
                            Symbol = "ETH",
                            Volume = 10,
                            PriceInstrumentSymbol = "ETHUSD",
                            DirectInstrumentPrice = true,
                            TargetRebalanceWeight = 0.5m
                        },
                    },
                StartPrice = 100m,
                StartTime = DateTime.UtcNow,
                QuoteInstrumentSymbol = "CICUSD",
                LastUpdate = default,
                LastRebalance = default,
                NextRebalance = default
            }
            );

            await _service.ApplySwapsAsync(new[]
            {
                ClientToBroker(1m, "CIC", 1000m, "USD", "SP-Broker"),
            });

            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("CIC").NetBalance.Should().Be(0m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").NetBalance.Should().Be(1m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").NetBalance.Should().Be(10m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("USD").NetBalance.Should().Be(-1000m);
        }


        [Test]
        public async Task ApplySwapWithIndexInstrument_2()
        {
            _indexAssetDictionaryClientMock.Data.Add("CIC", new IndexAsset
            {
                Symbol = "CIC",
                Broker = "jetwallet",
                Basket = new List<IndexAsset.BasketAsset>()
                    {
                        new IndexAsset.BasketAsset
                        {
                            Symbol = "BTC",
                            Volume = 1,
                            PriceInstrumentSymbol = "BTCUSD",
                            DirectInstrumentPrice = true,
                            TargetRebalanceWeight = 0.5m
                        },
                        new IndexAsset.BasketAsset
                        {
                            Symbol = "ETH",
                            Volume = 10,
                            PriceInstrumentSymbol = "ETHUSD",
                            DirectInstrumentPrice = true,
                            TargetRebalanceWeight = 0.5m
                        },
                    },
                StartPrice = 100m,
                StartTime = DateTime.UtcNow,
                QuoteInstrumentSymbol = "CICUSD",
                LastUpdate = default,
                LastRebalance = default,
                NextRebalance = default
            }
            );

            _indexAssetDictionaryClientMock.Data.Add("CIN", new IndexAsset
            {
                Symbol = "CIN",
                Broker = "jetwallet",
                Basket = new List<IndexAsset.BasketAsset>()
                    {
                        new IndexAsset.BasketAsset
                        {
                            Symbol = "BTC",
                            Volume = 1,
                            PriceInstrumentSymbol = "BTCUSD",
                            DirectInstrumentPrice = true,
                            TargetRebalanceWeight = 0.5m
                        },
                        new IndexAsset.BasketAsset
                        {
                            Symbol = "SOL",
                            Volume = 10,
                            PriceInstrumentSymbol = "SOLUSD",
                            DirectInstrumentPrice = true,
                            TargetRebalanceWeight = 0.5m
                        },
                        new IndexAsset.BasketAsset
                        {
                            Symbol = "XLM",
                            Volume = 20,
                            PriceInstrumentSymbol = "XLMUSD",
                            DirectInstrumentPrice = true,
                            TargetRebalanceWeight = 0.5m
                        },
                    },
                StartPrice = 100m,
                StartTime = DateTime.UtcNow,
                QuoteInstrumentSymbol = "CINUSD",
                LastUpdate = default,
                LastRebalance = default,
                NextRebalance = default
            }
            );


            await _service.ApplySwapsAsync(new[]
            {
                ClientToBroker(1m, "CIC", 1m, "CIN", "SP-Broker"),
            });

            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("CIC").NetBalance.Should().Be(0m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("CIN").NetBalance.Should().Be(0m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("BTC").NetBalance.Should().Be(0m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("ETH").NetBalance.Should().Be(10m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("SOL").NetBalance.Should().Be(-10m);
            _service.GetCurrentPortfolio().GetOrCreateAssetBySymbol("XLM").NetBalance.Should().Be(-20m);
        }
    }
}
