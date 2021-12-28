using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Domain.Models
{
    [DataContract]
    public class FeeShareSettlement
    {
        public const string TopicName = "jetwallet-liquidity-trading-portfolio-feesharesettlement";

        [DataMember(Order = 1)] public string OperationId { get; set; }  // OperationId
        [DataMember(Order = 2)] public string BrokerId { get; set; }  // BrokerId
        [DataMember(Order = 3)] public string WalletFrom { get; set; } // ConverterWalletId
        [DataMember(Order = 4)] public string WalletTo { get; set; }  // FeeShareWalletId
        [DataMember(Order = 5)] public string Asset { get; set; }  // FeeShareAsset
        [DataMember(Order = 6)] public decimal Volume { get; set; }  // FeeShareAmountInTargetAsset
        [DataMember(Order = 7)] public string Comment { get; set; }  // Формируем сами
        [DataMember(Order = 8)] public string ReferrerClientId { get; set; } // ReferrerClientId
        [DataMember(Order = 9)] public DateTime SettlementDate { get; set; } // DateTime.Now

    }
}
