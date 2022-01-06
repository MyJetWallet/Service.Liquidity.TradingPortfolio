using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Grpc.Models
{
    [DataContract]
    public class GetWalletsResponse
    {
        [DataMember(Order = 1)] public List<PortfolioWallet> Wallets{ get; set; }
    }
}