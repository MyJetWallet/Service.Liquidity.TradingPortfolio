﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MyJetWallet.Domain.Orders;

namespace Service.Liquidity.TradingPortfolio.Domain.Models
{
    [DataContract]
    public class AssetPortfolioTrade
    {
        public const string TopicName = "jetwallet-liquidity-portfolio-trades";
        [DataMember(Order = 1)] public long Id { get; set; }
        [DataMember(Order = 2)] public string TradeId { get; set; }
        [DataMember(Order = 3)] public string AssociateBrokerId { get; set; }
        [DataMember(Order = 4)] public string WalletName { get; set; }
        [DataMember(Order = 5)] public string AssociateSymbol { get; set; }
        [DataMember(Order = 6)] public string BaseAsset { get; set; }
        [DataMember(Order = 7)] public string QuoteAsset { get; set; }
        [DataMember(Order = 8)] public OrderSide Side { get; set; }
        [DataMember(Order = 9)] public decimal Price { get; set; }
        [DataMember(Order = 10)] public decimal BaseVolume { get; set; }
        [DataMember(Order = 11)] public decimal QuoteVolume { get; set; }
        [DataMember(Order = 12)] public decimal BaseVolumeInUsd { get; set; }
        [DataMember(Order = 13)] public decimal QuoteVolumeInUsd { get; set; }
        [DataMember(Order = 14)] public decimal BaseAssetPriceInUsd { get; set; }
        [DataMember(Order = 15)] public decimal QuoteAssetPriceInUsd { get; set; }
        [DataMember(Order = 16)] public DateTime DateTime { get; set; }
        [DataMember(Order = 17)] public string ErrorMessage { get; set; }
        [DataMember(Order = 18)] public string Source { get; set; }
        [DataMember(Order = 19)] public string Comment { get; set; }
        [DataMember(Order = 20)] public string User { get; set; }
        [DataMember(Order = 21)] public decimal TotalReleasePnl { get; set; }
        [DataMember(Order = 22)] public string FeeAsset { get; set; }
        [DataMember(Order = 23)] public decimal FeeVolume { get; set; }

        public AssetPortfolioTrade(string tradeId,
            string associateBrokerId,
            string associateSymbol,
            string baseAsset,
            string quoteAsset,
            string walletName,
            OrderSide side,
            decimal price,
            decimal baseVolume,
            decimal quoteVolume,
            DateTime dateTime,
            string source,
            string feeAsset,
            decimal feeVolume)
        {
            TradeId = tradeId;
            AssociateBrokerId = associateBrokerId;
            WalletName = walletName;
            AssociateSymbol = associateSymbol;
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            Side = side;
            Price = price;
            BaseVolume = baseVolume;
            QuoteVolume = quoteVolume;
            DateTime = dateTime;
            Source = source;
            FeeAsset = feeAsset;
            FeeVolume = feeVolume;
        }

        public AssetPortfolioTrade(string associateBrokerId,
            string associateSymbol,
            string baseAsset,
            string quoteAsset,
            string walletName,
            decimal price, decimal baseVolume,
            decimal quoteVolume, string comment, string user, string source,
            string feeAsset,
            decimal feeVolume)
        {
            AssociateBrokerId = associateBrokerId;
            WalletName = walletName;
            AssociateSymbol = associateSymbol;
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            Price = price;
            BaseVolume = baseVolume;
            QuoteVolume = quoteVolume;
            Source = source;
            Comment = comment;
            User = user;
            FeeAsset = feeAsset;
            FeeVolume = feeVolume;

            TradeId = Guid.NewGuid().ToString("N");
            DateTime = DateTime.UtcNow;
            Side = baseVolume < 0 ? OrderSide.Sell : OrderSide.Buy;
        }

        public AssetPortfolioTrade()
        {
        }
    }
}
