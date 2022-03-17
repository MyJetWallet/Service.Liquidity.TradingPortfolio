using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests.Mocks;

public class PortfolioChangeBalancePublisherMock: IServiceBusPublisher<PortfolioChangeBalance>
{
    private Action<PortfolioChangeBalance> Callback { get; set; }
    
    public Task PublishAsync(PortfolioChangeBalance message)
    {
        Callback?.Invoke(message);
        
        return Task.CompletedTask;
    }

    public Task PublishAsync(IEnumerable<PortfolioChangeBalance> messageList)
    {
        foreach (var message in messageList)
        {
            Callback?.Invoke(message);
        }
        
        return Task.CompletedTask;
    }
}