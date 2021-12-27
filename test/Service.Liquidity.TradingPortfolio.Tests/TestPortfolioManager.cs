using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain;

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
            
            return null;
        }
    }

    public class TestPortfolioManager
    {
        public PortfolioManager _service;

        [SetUp]
        public void Setup()
        {
            _service = new PortfolioManager( new PortfolioWalletManagerMock());
        }

        [Test]
        public async Task ApplyClientToBrokerSwap()
        {
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

            await _service.ApplySwaps(new[] { swaps });
            var portfolio = _service.GetCurrentPortfolio();

            portfolio.Assets["BTC"].WalletBalances["Converter"].Balance.Should().Be(1m);
            portfolio.Assets["USD"].WalletBalances["Converter"].Balance.Should().Be(-50000m);
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

            await _service.ApplySwaps(new[] { swaps });
            var portfolio = _service.GetCurrentPortfolio();

            portfolio.GetAssetBySymbol("BTC").GetByPortfolioWalletId("Converter").Should().BeNull();
            portfolio.GetAssetBySymbol("USD").GetByPortfolioWalletId("Converter").Should().BeNull();
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

            await _service.ApplySwaps(new[] { swaps });
            var portfolio = _service.GetCurrentPortfolio();

            portfolio.Assets["BTC"].WalletBalances["Converter"].Balance.Should().Be(-1m);
            portfolio.Assets["USD"].WalletBalances["Converter"].Balance.Should().Be(50000m);
        }


        [Test]
        public async Task ApplySeveralSwaps()
        {
            await _service.ApplySwaps(new[] 
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
            await _service.ApplySwaps(new[]
            {
                ClientToBroker(1m, "BTC", 40000m, "USD"),
                ClientToBroker(40000m, "USD", 10m, "ETH"),
            });

            var portfolio = _service.GetCurrentPortfolio();

            portfolio.Assets["BTC"].WalletBalances["Converter"].Balance.Should().Be(1m);
            portfolio.Assets["USD"].WalletBalances["Converter"].Balance.Should().Be(0m);
            portfolio.Assets["ETH"].WalletBalances["Converter"].Balance.Should().Be(-10m);
        }

        private SwapMessage ClientToBroker(decimal vol1, string asset1, decimal vol2, string asset2)
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
                WalletId2 = "SP-Broker",
                Id = "1",
                MessageId = "1",
                Timestamp = DateTime.Now,
                DifferenceAsset = "USD",
                DifferenceVolumeAbs = 50m,
            };
        }
    }
}
