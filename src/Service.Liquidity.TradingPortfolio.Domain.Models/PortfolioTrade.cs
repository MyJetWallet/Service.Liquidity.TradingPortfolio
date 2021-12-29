using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MyJetWallet.Domain.Orders;

namespace Service.Liquidity.TradingPortfolio.Domain.Models
{
    [DataContract]
    public class PortfolioTrade
    {
        public const string TopicName = "jetwallet-liquidity-trading-portfolio-trades";
        [DataMember(Order = 1)] public string TradeId { get; set; }  // TradeMessage.Id & SwapMessage.Id
        [DataMember(Order = 2)] public string AssociateBrokerId { get; set; }  // TradeMessage.Id & SwapMessage.BrokerId
        [DataMember(Order = 3)] public string WalletName { get; set; }  // PortfolioWalletId
        [DataMember(Order = 4)] public string AssociateSymbol { get; set; }  //TradeMessage.AssociateSymbol   SwapMessage.AssetId1|AssetId2
        [DataMember(Order = 5)] public string BaseAsset { get; set; } //TradeMessage.BaseAsset   SwapMessage.AssetId1
        [DataMember(Order = 6)] public string QuoteAsset { get; set; } //TradeMessage.BaseAsset   SwapMessage.AssetId2
        [DataMember(Order = 7)] public OrderSide Side { get; set; }  //TradeMessage.BaseAsset   SwapMessage.Buy проверить сторону 
        [DataMember(Order = 8)] public decimal Price { get; set; }  //TradeMessage.BaseAsset   SwapMessage.Buy проверить сторону 
        [DataMember(Order = 9)] public decimal BaseVolume { get; set; }
        [DataMember(Order = 10)] public decimal QuoteVolume { get; set; }
        [DataMember(Order = 11)] public decimal BaseVolumeInUsd { get; set; } // из Portfolio
        [DataMember(Order = 12)] public decimal QuoteVolumeInUsd { get; set; }// из Portfolio
        [DataMember(Order = 13)] public decimal BaseAssetPriceInUsd { get; set; }// из Portfolio
        [DataMember(Order = 14)] public decimal QuoteAssetPriceInUsd { get; set; }// из Portfolio
        [DataMember(Order = 15)] public DateTime DateTime { get; set; } //Now
        [DataMember(Order = 16)] public string Source { get; set; } //TradeMessage.Source   SwapMessage."Converter"
        [DataMember(Order = 17)] public string Comment { get; set; }//TradeMessage.Comment   SwapMessage."Swap with client"
        [DataMember(Order = 18)] public string User { get; set; } // User   SwapMessage."Converter"
        [DataMember(Order = 19)] public string FeeAsset { get; set; }  //TradeMessage.FeeAsset    SwapMessage.AssetId2
        [DataMember(Order = 20)] public decimal FeeVolume { get; set; }//TradeMessage.FeeVolume    SwapMessage.0

        public PortfolioTrade(string tradeId,
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

        public PortfolioTrade(string associateBrokerId,
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

        public PortfolioTrade()
        {
        }
    }
}
