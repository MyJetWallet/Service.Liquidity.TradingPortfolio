﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests;

public class PortfolioPublisherMock : IServiceBusPublisher<Portfolio>
{
    public Action<Portfolio> Callback { get; set; }

    public async Task PublishAsync(Portfolio message)
    {
        Callback?.Invoke(message);
    }

    public async Task PublishAsync(IEnumerable<Portfolio> messageList)
    {
        foreach (var message in messageList)
        {
            Callback?.Invoke(message);
        }
    }
}