using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests;

public class PortfolioManualSettlementPublisherMock : IServiceBusPublisher<PortfolioSettlement>
{
    public Action<PortfolioSettlement> Callback { get; set; }

    public async Task PublishAsync(PortfolioSettlement message)
    {
        Callback?.Invoke(message);
    }

    public async Task PublishAsync(IEnumerable<PortfolioSettlement> messageList)
    {
        foreach (var message in messageList)
        {
            Callback?.Invoke(message);
        }
    }
}