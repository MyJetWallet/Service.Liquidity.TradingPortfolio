using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Grpc.Models
{
    [DataContract]
    public class BalanceRequest
    {
        [DataMember(Order = 1)] public string Wallet { get; set; }
        [DataMember(Order = 2)] public string Asset { get; set; }
        [DataMember(Order = 3)] public decimal Balance { get; set; }
        [DataMember(Order = 4)] public string Comment { get; set; }
        [DataMember(Order = 5)] public string User  { get; set; }
    }
}