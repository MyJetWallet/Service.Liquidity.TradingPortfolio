using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Domain.Models
{
    [DataContract]
    public class PortfolioWallet
    {
        [DataMember(Order = 1)] public bool IsInternal { get; set; }
        [DataMember(Order = 2)] public string Name { get; set; }
        [DataMember(Order = 3)] public string WalletId { get; set; }
        [DataMember(Order = 4)] public string ExternalSource { get; set; }
        [DataMember(Order = 5)] public string BrokerId { get; set; }
    }
}
