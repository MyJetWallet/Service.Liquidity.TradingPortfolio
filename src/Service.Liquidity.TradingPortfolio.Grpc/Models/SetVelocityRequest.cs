using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Grpc.Models
{
    [DataContract]
    public class SetVelocityRequest
    {
        [DataMember(Order = 1)] public string Broker { get; set; }
        [DataMember(Order = 2)] public string Wallet { get; set; }
        [DataMember(Order = 3)] public string Asset { get; set; }
        [DataMember(Order = 4)] public decimal Velocity { get; set; }
    }
}