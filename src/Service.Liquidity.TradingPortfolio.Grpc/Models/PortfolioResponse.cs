using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Runtime.Serialization;

namespace Service.Liquidity.TradingPortfolio.Grpc
{
    [DataContract]
    public class PortfolioResponse
    {
        [DataMember(Order = 1)] public Portfolio Portfolio { get; set; }
    }
}