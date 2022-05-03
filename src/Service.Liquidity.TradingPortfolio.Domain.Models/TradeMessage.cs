using System;
using System.Runtime.Serialization;
using MyJetWallet.Domain.Orders;

namespace Service.Liquidity.TradingPortfolio.Domain.Models
{
    [DataContract]
    public class TradeMessage
    {
        [DataMember(Order = 1)]
        public string Id { get; set; }

        [DataMember(Order = 2)]
        public string ReferenceId { get; set; }

        [DataMember(Order = 3)]
        public string Market { get; set; }

        [DataMember(Order = 4)]
        public OrderSide Side { get; set; }

        [DataMember(Order = 5)]
        public decimal Price { get; set; }

        [DataMember(Order = 6)]
        public decimal Volume { get; set; }

        [DataMember(Order = 7)]
        public decimal OppositeVolume { get; set; }

        [DataMember(Order = 8)]
        public DateTime Timestamp { get; set; }

        [DataMember(Order = 9)]
        public string AssociateWalletId { get; set; }

        [DataMember(Order = 10)]
        public string AssociateBrokerId { get; set; }

        [DataMember(Order = 11)]
        public string AssociateClientId { get; set; }

        [DataMember(Order = 12)]
        public string AssociateSymbol { get; set; }

        [DataMember(Order = 13)]
        public string Source { get; set; }
        [DataMember(Order = 14)]
        public string BaseAsset { get; set; }
        [DataMember(Order = 15)]
        public string QuoteAsset { get; set; }
        [DataMember(Order = 16)]
        public string Comment { get; set; }
        [DataMember(Order = 17)]
        public string User { get; set; }
        
        [DataMember(Order = 18)] 
        public string FeeAsset { get; set; }
        
        [DataMember(Order = 19)] 
        public decimal FeeVolume { get; set; }

        [DataMember(Order = 20)] public PortfolioTradeType Type { get; set; } = PortfolioTradeType.None;
    }
}