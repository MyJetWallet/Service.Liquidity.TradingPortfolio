using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests.Mocks;

public class PortfolioPublisherMock : IServiceBusPublisher<Portfolio>
{
    private Action<Portfolio> Callback { get; set; }

    public Task PublishAsync(Portfolio message)
    {
        Callback?.Invoke(message);
        
        return Task.CompletedTask;
    }

    public Task PublishAsync(IEnumerable<Portfolio> messageList)
    {
        foreach (var message in messageList)
        {
            Callback?.Invoke(message);
        }
        
        return Task.CompletedTask;
    }
}