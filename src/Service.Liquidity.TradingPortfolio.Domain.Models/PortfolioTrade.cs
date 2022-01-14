using System;
using System.Runtime.Serialization;
using MyJetWallet.Domain.Orders;

namespace Service.Liquidity.TradingPortfolio.Domain.Models
{
    [DataContract]
    public class PortfolioTrade
    {
        public const string TopicName = "jetwallet-liquidity-tradingportfolio-trades";
        [DataMember(Order = 1)] public string TradeId { get; set; }  // TradeMessage.Id & SwapMessage.Id
        [DataMember(Order = 2)] public string AssociateBrokerId { get; set; }  // TradeMessage.Id & SwapMessage.BrokerId
        [DataMember(Order = 3)] public string BaseWalletName { get; set; }  // PortfolioWalletId
        [DataMember(Order = 4)] public string QuoteWalletName { get; set; }  // PortfolioWalletId
        [DataMember(Order = 5)] public string AssociateSymbol { get; set; }  //TradeMessage.AssociateSymbol   SwapMessage.AssetId1|AssetId2 =  swap.AssetId1 + "|" + swap.AssetId2,
        [DataMember(Order = 6)] public string BaseAsset { get; set; } //TradeMessage.BaseAsset   SwapMessage.AssetId1
        [DataMember(Order = 7)] public string QuoteAsset { get; set; } //TradeMessage.BaseAsset   SwapMessage.AssetId2
        [DataMember(Order = 8)] public OrderSide Side { get; set; }  //TradeMessage.BaseAsset   SwapMessage.Buy проверить сторону 
        [DataMember(Order = 9)] public decimal Price { get; set; }  //TradeMessage.BaseAsset   
        [DataMember(Order = 10)] public decimal BaseVolume { get; set; }
        [DataMember(Order = 11)] public decimal QuoteVolume { get; set; }
        [DataMember(Order = 12)] public decimal BaseVolumeInUsd { get; set; } // из Portfolio
        [DataMember(Order = 13)] public decimal QuoteVolumeInUsd { get; set; }// из Portfolio
        [DataMember(Order = 14)] public decimal BaseAssetPriceInUsd { get; set; }// из Portfolio
        [DataMember(Order = 15)] public decimal QuoteAssetPriceInUsd { get; set; }// из Portfolio
        [DataMember(Order = 16)] public DateTime DateTime { get; set; } //Now
        [DataMember(Order = 17)] public string Source { get; set; } //TradeMessage.Source   SwapMessage."Converter"
        [DataMember(Order = 18)] public string Comment { get; set; }//TradeMessage.Comment   SwapMessage."Swap with client"
        [DataMember(Order = 19)] public string User { get; set; } // User   SwapMessage."Converter"
        [DataMember(Order = 20)] public string FeeAsset { get; set; }  //TradeMessage.FeeAsset    SwapMessage.AssetId2
        [DataMember(Order = 21)] public decimal FeeVolume { get; set; }//TradeMessage.FeeVolume    SwapMessage.0
    }
}
