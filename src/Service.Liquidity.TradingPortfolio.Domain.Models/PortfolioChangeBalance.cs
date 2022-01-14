using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Domain.Models
{
    [DataContract]
    public class PortfolioChangeBalance
    {
        public const string TopicName = "jetwallet-liquidity-tradingportfolio-changebalance";

        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string WalletName { get; set; }
        [DataMember(Order = 3)] public string Asset { get; set; }
        [DataMember(Order = 4)] public decimal Balance { get; set; }
        [DataMember(Order = 5)] public DateTime UpdateDate { get; set; }
        [DataMember(Order = 6)] public string Comment { get; set; }
        [DataMember(Order = 7)] public string User { get; set; }
        [DataMember(Order = 8)] public decimal BalanceBeforeUpdate { get; set; }
        [DataMember(Order = 100)] public long Id { get; set; }
    }
}
