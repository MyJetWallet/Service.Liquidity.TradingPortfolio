using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests.Mocks;

public class PortfolioTraderPublisherMock : IServiceBusPublisher<PortfolioTrade>
{
    private Action<PortfolioTrade> Callback { get; set; }

    public Task PublishAsync(PortfolioTrade message)
    {
        Callback?.Invoke(message);
        return Task.CompletedTask;
    }

    public Task PublishAsync(IEnumerable<PortfolioTrade> messageList)
    {
        foreach (var message in messageList)
        {
            Callback?.Invoke(message);
        }

        return Task.CompletedTask;
    }
}