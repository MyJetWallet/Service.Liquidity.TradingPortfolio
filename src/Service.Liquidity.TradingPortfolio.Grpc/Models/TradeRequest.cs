using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Grpc.Models;

[DataContract]
public class TradeRequest
{
    [DataMember(Order = 1)] public string BrokerId { get; set; }
    [DataMember(Order = 2)] public string WalletName { get; set; }
    [DataMember(Order = 3)] public string AssociateSymbol { get; set; }
    [DataMember(Order = 4)] public decimal Price { get; set; }
    [DataMember(Order = 5)] public decimal BaseVolume { get; set; }
    [DataMember(Order = 6)] public decimal QuoteVolume { get; set; }
    [DataMember(Order = 7)] public string Comment { get; set; }
    [DataMember(Order = 8)] public string User { get; set; }
    [DataMember(Order = 9)] public string FeeAsset { get; set; }
    [DataMember(Order = 10)] public decimal FeeVolume { get; set; }
    [DataMember(Order = 11)] public string BaseAsset { get; set; }
    [DataMember(Order = 12)] public string QuoteAsset { get; set; }
}