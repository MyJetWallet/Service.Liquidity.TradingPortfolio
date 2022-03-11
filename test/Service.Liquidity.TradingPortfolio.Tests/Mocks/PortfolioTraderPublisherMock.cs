using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests;

public class PortfolioTraderPublisherMock : IServiceBusPublisher<PortfolioTrade>
{
    public Action<PortfolioTrade> Callback { get; set; }

    public async Task PublishAsync(PortfolioTrade message)
    {
        Callback?.Invoke(message);
    }

    public async Task PublishAsync(IEnumerable<PortfolioTrade> messageList)
    {
        foreach (var message in messageList)
        {
            Callback?.Invoke(message);
        }
    }
}