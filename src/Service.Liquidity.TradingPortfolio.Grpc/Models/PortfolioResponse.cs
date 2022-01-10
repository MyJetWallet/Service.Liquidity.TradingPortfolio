using System.Runtime.Serialization;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Grpc.Models
{
    [DataContract]
    public class PortfolioResponse
    {
        [DataMember(Order = 1)] public Portfolio Portfolio { get; set; }
    }
}