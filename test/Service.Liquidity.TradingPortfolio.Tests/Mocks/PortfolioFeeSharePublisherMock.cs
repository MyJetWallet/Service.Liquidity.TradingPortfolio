using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests;

public class PortfolioFeeSharePublisherMock : IServiceBusPublisher<PortfolioFeeShare>
{
    public Action<PortfolioFeeShare> Callback { get; set; }

    public async Task PublishAsync(PortfolioFeeShare message)
    {
        Callback?.Invoke(message);
    }

    public async Task PublishAsync(IEnumerable<PortfolioFeeShare> messageList)
    {
        foreach (var message in messageList)
        {
            Callback?.Invoke(message);
        }
    }
}