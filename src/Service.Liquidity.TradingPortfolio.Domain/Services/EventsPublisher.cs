using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.TradingPortfolio.Domain.Interfaces;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Domain.Services;

public class EventsPublisher : IEventsPublisher
{
    private readonly IServiceBusPublisher<Portfolio> _portfolioPublisher;
    private readonly IServiceBusPublisher<PortfolioFeeShare> _feeSharePublisher;
    private readonly IServiceBusPublisher<PortfolioTrade> _tradePublisher;
    private readonly IServiceBusPublisher<PortfolioSettlement> _settlementPublisher;
    private readonly IServiceBusPublisher<PortfolioChangeBalance> _changeBalancePublisher;

    public EventsPublisher(
        IServiceBusPublisher<Portfolio> portfolioPublisher,
        IServiceBusPublisher<PortfolioFeeShare> feeSharePublisher,
        IServiceBusPublisher<PortfolioTrade> tradePublisher,
        IServiceBusPublisher<PortfolioSettlement> settlementPublisher,
        IServiceBusPublisher<PortfolioChangeBalance> changeBalancePublisher
    )
    {
        _portfolioPublisher = portfolioPublisher;
        _feeSharePublisher = feeSharePublisher;
        _tradePublisher = tradePublisher;
        _settlementPublisher = settlementPublisher;
        _changeBalancePublisher = changeBalancePublisher;
    }

    public async Task PublishAsync(Portfolio eventModel)
    {
        await _portfolioPublisher.PublishAsync(eventModel);
    }

    public async Task PublishAsync(PortfolioFeeShare eventModel)
    {
        await _feeSharePublisher.PublishAsync(eventModel);
    }

    public async Task PublishAsync(IEnumerable<PortfolioTrade> eventModel)
    {
        await _tradePublisher.PublishAsync(eventModel);
    }

    public async Task PublishAsync(PortfolioSettlement eventModel)
    {
        await _settlementPublisher.PublishAsync(eventModel);
    }

    public async Task PublishAsync(PortfolioChangeBalance eventModel)
    {
        await _changeBalancePublisher.PublishAsync(eventModel);
    }
}