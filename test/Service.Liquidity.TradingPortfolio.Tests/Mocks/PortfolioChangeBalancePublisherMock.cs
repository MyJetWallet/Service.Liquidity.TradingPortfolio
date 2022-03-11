using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests;

public class PortfolioChangeBalancePublisherMock: IServiceBusPublisher<PortfolioChangeBalance>
{
    public Action<PortfolioChangeBalance> Callback { get; set; }
    public async Task PublishAsync(PortfolioChangeBalance message)
    {
        Callback?.Invoke(message);
    }

    public async Task PublishAsync(IEnumerable<PortfolioChangeBalance> messageList)
    {
        foreach (var message in messageList)
        {
            Callback?.Invoke(message);
        }
    }
}