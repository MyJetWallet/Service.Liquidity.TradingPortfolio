using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests.Mocks;

public class PortfolioManualSettlementPublisherMock : IServiceBusPublisher<PortfolioSettlement>
{
    private Action<PortfolioSettlement> Callback { get; set; }

    public Task PublishAsync(PortfolioSettlement message)
    {
        Callback?.Invoke(message);
        
        return Task.CompletedTask;
    }

    public Task PublishAsync(IEnumerable<PortfolioSettlement> messageList)
    {
        foreach (var message in messageList)
        {
            Callback?.Invoke(message);
        }

        return Task.CompletedTask;
    }
}