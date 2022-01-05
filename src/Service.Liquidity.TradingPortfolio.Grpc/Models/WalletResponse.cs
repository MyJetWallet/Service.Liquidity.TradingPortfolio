using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Grpc.Models
{
    [DataContract]
    public class WalletResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    }
}