using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Domain.Models
{

    [DataContract]
    public class Portfolio
    {
        public const string TopicName = "jetwallet-liquidity-tradingportfolio-portfolio";

        [DataMember(Order = 1)] public Dictionary<string, Asset> Assets { get; set; }
        [DataMember(Order = 2)] public decimal TotalNetInUsd { get; set; }
        [DataMember(Order = 3)] public decimal TotalDailyVelocityRiskInUsd { get; set; }
        [DataMember(Order = 4)] public decimal TotalNegativeNetInUsd { get; set; }
        [DataMember(Order = 5)] public decimal TotalNegativeNetPercent { get; set; }
        [DataMember(Order = 6)] public decimal TotalPositiveNetInUsd { get; set; }
        [DataMember(Order = 7)] public decimal TotalPositiveNetInPercent { get; set; }
        [DataMember(Order = 8)] public decimal TotalLeverage { get; set; }
        [DataMember(Order = 9)] public decimal InternalBalanceInUsd { get; set; }
        [DataMember(Order = 10)] public decimal ExternalBalanceInUsd { get; set; }

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
        
        public Asset GetAssetBySymbol(string symbol)
        {
            if (!Assets.TryGetValue(symbol, out var asset))
            {
                return null;
            }
            return asset;
        }

        [DataContract]
        public class Asset
        {
            [DataMember(Order = 1)] public string Symbol { get; set; }
            [DataMember(Order = 2)] public Dictionary<string, WalletBalance> WalletBalances { get; set; } 
            [DataMember(Order = 3)] public decimal NetBalance { get; set; }
            [DataMember(Order = 4)] public decimal NetBalanceInUsd { get; set; }
            [DataMember(Order = 5)] public decimal DailyVelocity { get; set; }
            [DataMember(Order = 6)] public decimal DailyVelocityRiskInUsd { get; set; }
            [DataMember(Order = 7)] public decimal DailyVelocityLowOpen { get; set; }
            [DataMember(Order = 8)] public decimal DailyVelocityHighOpen { get; set; }
            [DataMember(Order = 9)] public decimal NetInternalBalance { get; set; }
            [DataMember(Order = 10)] public decimal NetInternalBalanceInUsd { get; set; }
            [DataMember(Order = 11)] public decimal NetExternalBalance { get; set; }
            [DataMember(Order = 12)] public decimal NetExternalBalanceInUsd { get; set; }
            
            public WalletBalance GetOrCreateWalletBalance(PortfolioWallet portfolioWallet)
            {
                if (!WalletBalances.TryGetValue(portfolioWallet.Name, out var walletBalance))
                {
                    walletBalance = new Portfolio.WalletBalance()
                    {
                        Balance = 0m,
                        BalanceInUsd = 0m,
                        Wallet = new PortfolioWallet()
                        {
                            Name = portfolioWallet.Name,
                            BrokerId = portfolioWallet.BrokerId,
                            ExternalSource = portfolioWallet.ExternalSource,
                            IsInternal = portfolioWallet.IsInternal,
                            WalletId = portfolioWallet.WalletId,
                        },
                    };
                    WalletBalances[portfolioWallet.Name] = walletBalance;
                }
                return walletBalance;
            }

            public WalletBalance GetWalletBalanceByPortfolioWalletName(string walletName)
            {
                if (!WalletBalances.TryGetValue(walletName, out var walletBalance))
                {
                    return null;
                }
                return walletBalance;
            }
        }

        public Portfolio MakeCopy()
        {
            return Helper.CloneJson(this);
        }

        [DataContract]
        public class WalletBalance
        {
            [DataMember(Order = 1)] public PortfolioWallet Wallet { get; set; }
            [DataMember(Order = 2)] public decimal Balance { get; set; }
            [DataMember(Order = 3)] public decimal BalanceInUsd { get; set; }
        }
    }
}
