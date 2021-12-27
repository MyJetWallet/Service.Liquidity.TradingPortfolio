using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MyJetWallet.Domain.Orders;

namespace Service.Liquidity.TradingPortfolio.Domain.Models
{
    [DataContract]
    public class Portfolio
    {
        public const string TopicName = "jetwallet-liquidity-trading-portfolio";

        //TODO: Add [DataMember(Order = 1)]
        public Dictionary<string, Asset> Assets { get; set; }
        
        public decimal TotalNetInUsd { get; set; }

        public decimal TotalDailyVelocityRiskInUsd { get; set; }

        public Asset GetOrCreateAssetBySymbol(string symbol)
        {
            if (!Assets.TryGetValue(symbol, out var asset))
            {
                asset = new Portfolio.Asset() 
                { 
                    Symbol = symbol,
                    WalletBalances = new Dictionary<string, WalletBalance>()
                };
                Assets[symbol] = asset;
            }
            return asset;
        }

        public class Asset
        {
            public string Symbol { get; set; }
            public Dictionary<string, WalletBalance> WalletBalances { get; set; }
            public decimal NetBalance { get; set; }
            public decimal NetBalanceInUsd { get; set; }
            public decimal DailyVelocity { get; set; }
            public decimal DailyVelocityRiskInUsd { get; set; }

            public WalletBalance GetOrCreate(PortfolioWallet portfolioWallet)
            {
                if (!WalletBalances.TryGetValue(portfolioWallet.Id, out var walletBalance))
                {
                    walletBalance = new Portfolio.WalletBalance()
                    {
                        Balance = 0,
                        Wallet = portfolioWallet
                    };
                    WalletBalances[portfolioWallet.Id] = walletBalance;
                }
                return walletBalance;
            }

            public WalletBalance GetByPortfolioWalletId(string portfolioWalletId)
            {
                if (!WalletBalances.TryGetValue(portfolioWalletId, out var walletBalance))
                {
                    return null;
                }
                return walletBalance;
            }

        }

        public class WalletBalance
        {
            public PortfolioWallet Wallet { get; set; }
            public decimal Balance { get; set; }
            public decimal BalanceInUsd { get; set; }
        }
    }
}
