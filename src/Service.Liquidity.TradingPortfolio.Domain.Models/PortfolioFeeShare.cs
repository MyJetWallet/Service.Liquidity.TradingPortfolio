using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Domain.Models
{
    [DataContract]
    public class PortfolioFeeShare
    {
        public const string TopicName = "jetwallet-liquidity-tradingportfolio-feeshare";

        [DataMember(Order = 1)] public string OperationId { get; set; }  // OperationId
        [DataMember(Order = 2)] public string BrokerId { get; set; }  // BrokerId
        [DataMember(Order = 3)] public string WalletFrom { get; set; } // ConverterWalletId
        [DataMember(Order = 4)] public string WalletTo { get; set; }  // FeeShareWalletId
        [DataMember(Order = 5)] public string Asset { get; set; }  // FeeShareAsset
        [DataMember(Order = 6)] public decimal VolumeFrom { get; set; }  // FeeShareAmountInTargetAsset
        [DataMember(Order = 7)] public decimal VolumeTo { get; set; }  // FeeShareAmountInTargetAsset
        [DataMember(Order = 8)] public string Comment { get; set; }  // Формируем сами
        [DataMember(Order = 9)] public string ReferrerClientId { get; set; } // ReferrerClientId
        [DataMember(Order = 10)] public DateTime SettlementDate { get; set; } // DateTime.Now
        [DataMember(Order = 100)] public long Id { get; set; }
    }
}
