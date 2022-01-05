﻿using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Grpc
{
    [DataContract]
    public class WalletAddRequest
    {
        [DataMember(Order = 1)] public string WalletIdName { get; set; }
        [DataMember(Order = 2)] public string WalletId { get; set; }
        [DataMember(Order = 3)] public string BrokerId { get; set; }
        [DataMember(Order = 4)] public string ClientId { get; set; }
        [DataMember(Order = 5)] public string Source { get; set; }
    }
}