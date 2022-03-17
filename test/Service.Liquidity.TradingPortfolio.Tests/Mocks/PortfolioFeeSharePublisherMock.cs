using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests.Mocks;

public class PortfolioFeeSharePublisherMock : IServiceBusPublisher<PortfolioFeeShare>
{
    private Action<PortfolioFeeShare> Callback { get; set; }

    public Task PublishAsync(PortfolioFeeShare message)
    {
        Callback?.Invoke(message);
        return Task.CompletedTask;
    }

    public Task PublishAsync(IEnumerable<PortfolioFeeShare> messageList)
    {
        foreach (var message in messageList)
        {
            Callback?.Invoke(message);
        }

        return Task.CompletedTask;
    }
}