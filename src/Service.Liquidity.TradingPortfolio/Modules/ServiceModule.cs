﻿using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.FeeShareEngine.Domain.Models.Models;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.PortfolioHedger.Client;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using Service.Liquidity.TradingPortfolio.Subscribers;

namespace Service.Liquidity.TradingPortfolio.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //Subscribers
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(
                Program.ReloadedSettings(e => e.SpotServiceBusHostPort), 
                Program.LogFactory);

            builder.RegisterMyServiceBusSubscriberSingle<FeeShareEntity>(serviceBusClient, 
                FeeShareEntity.TopicName, 
                $"TradingPortfolio", 
                TopicQueueType.PermanentWithSingleConnection);

            builder.RegisterMyServiceBusSubscriberBatch<SwapMessage>(serviceBusClient,
                SwapMessage.TopicName,
                $"TradingPortfolio",
                TopicQueueType.PermanentWithSingleConnection);

            builder.RegisterPortfolioHedgerServiceBusClient(serviceBusClient,
                $"TradingPortfolio",
                TopicQueueType.PermanentWithSingleConnection,
                true);
            
            //Publishers
            builder.RegisterMyServiceBusPublisher<PortfolioTrade>(serviceBusClient, PortfolioTrade.TopicName, true);
            builder.RegisterMyServiceBusPublisher<ChangeBalanceHistory>(serviceBusClient, ChangeBalanceHistory.TopicName, true);
            builder.RegisterMyServiceBusPublisher<ManualSettlement>(serviceBusClient, ManualSettlement.TopicName, true);
            builder.RegisterMyServiceBusPublisher<FeeShareSettlement>(serviceBusClient, FeeShareSettlement.TopicName, true);
            
            builder.RegisterMyServiceBusPublisher<Portfolio>(serviceBusClient, Portfolio.TopicName, true);

            builder.RegisterType<SwapMessageSubscriber>().SingleInstance().AutoActivate();
            builder.RegisterType<PortfolioManager>().SingleInstance().As<IPortfolioManager>().AutoActivate();
        }
    }
}