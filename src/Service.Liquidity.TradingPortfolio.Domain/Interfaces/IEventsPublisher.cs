using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Domain.Interfaces;

public interface IEventsPublisher
{
    Task PublishAsync(Portfolio eventModel);
    Task PublishAsync(PortfolioFeeShare eventModel);
    Task PublishAsync(IEnumerable<PortfolioTrade> eventModel);
    Task PublishAsync(PortfolioSettlement eventModel);
    Task PublishAsync(PortfolioChangeBalance eventModel);
}