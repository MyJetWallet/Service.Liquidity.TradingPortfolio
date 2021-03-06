using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Domain.Models
{
    [DataContract]
    public class PortfolioSettlement
    {
        public const string TopicName = "jetwallet-liquidity-tradingportfolio-settlement";

        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string WalletFrom { get; set; }
        [DataMember(Order = 3)] public string WalletTo { get; set; }
        [DataMember(Order = 4)] public string Asset { get; set; }
        [DataMember(Order = 5)] public decimal VolumeFrom { get; set; }
        [DataMember(Order = 6)] public decimal VolumeTo { get; set; }
        [DataMember(Order = 7)] public string Comment { get; set; }
        [DataMember(Order = 8)] public string User { get; set; }
        [DataMember(Order = 9)] public DateTime SettlementDate { get; set; }
        [DataMember(Order = 10)] public Decimal ReleasedPnl { get; set; }
        [DataMember(Order = 100)] public long Id { get; set; }
    }
}
