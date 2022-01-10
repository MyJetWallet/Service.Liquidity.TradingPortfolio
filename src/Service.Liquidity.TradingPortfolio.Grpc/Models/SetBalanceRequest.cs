﻿using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Grpc.Models
{
    [DataContract]
    public class SetBalanceRequest
    {
        [DataMember(Order = 1)] public string Wallet { get; set; }
        [DataMember(Order = 2)] public string Asset { get; set; }
        [DataMember(Order = 3)] public decimal Balance { get; set; }
    }
}