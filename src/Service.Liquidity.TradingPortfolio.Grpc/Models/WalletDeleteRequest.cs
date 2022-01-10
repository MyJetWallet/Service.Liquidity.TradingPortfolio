using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Grpc.Models
{
    [DataContract]
    public class WalletDeleteRequest
    {
        [DataMember(Order = 1)] public string WalletIdName { get; set; }
        [DataMember(Order = 2)] public string WalletId { get; set; }
        [DataMember(Order = 3)] public string BrokerId { get; set; }
    }
}