using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Grpc.Models
{
    [DataContract]
    public class SetVelocityRequest
    {
        [DataMember(Order = 1)] public string Broker { get; set; }
        [DataMember(Order = 2)] public string Wallet { get; set; }
        [DataMember(Order = 3)] public string Asset { get; set; }
        [Obsolete("Velocity is obsolete, use VelocityLowOpen and VelocityHighOpen", false)]
        [DataMember(Order = 4)] public decimal Velocity { get; set; }
        [DataMember(Order = 5)] public decimal User { get; set; }
        [DataMember(Order = 7)] public decimal VelocityLowOpen { get; set; }
        [DataMember(Order = 8)] public decimal VelocityHighOpen { get; set; }
    }
}