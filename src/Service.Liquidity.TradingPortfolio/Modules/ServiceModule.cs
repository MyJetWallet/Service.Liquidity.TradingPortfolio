using Autofac;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.FeeShareEngine.Domain.Models.Models;
using Service.Liquidity.Converter.Domain.Models;
using Service.Liquidity.Hedger.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.Liquidity.TradingPortfolio.Domain.Interfaces;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;
using Service.Liquidity.TradingPortfolio.Domain.Services;
using Service.Liquidity.TradingPortfolio.Grpc;
using Service.Liquidity.TradingPortfolio.Services;
using Service.Liquidity.TradingPortfolio.Subscribers;

namespace Service.Liquidity.TradingPortfolio.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterServiceBus(builder);
            RegisterSubscribers(builder);

            //Services
            builder.RegisterType<PortfolioManager>().SingleInstance().As<IPortfolioManager>().AutoActivate().AsSelf();
            builder.RegisterType<PortfolioWalletManager>().SingleInstance().As<IPortfolioWalletManager>().AutoActivate()
                .AsSelf();
            builder.RegisterType<ManualInputService>().As<IManualInputService>();
            builder.RegisterType<PortfolioWalletsNoSqlStorage>().As<IPortfolioWalletsStorage>()
                .SingleInstance().AutoActivate();

            //MyNoSql
            builder.RegisterMyNoSqlWriter<PortfolioWalletNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                PortfolioWalletNoSql.TableName);
            builder.RegisterMyNoSqlWriter<PortfolioNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                PortfolioNoSql.TableName);
        }

        private static void RegisterSubscribers(ContainerBuilder builder)
        {
            builder.RegisterType<FeeShareMessageSubscriber>().As<IStartable>().SingleInstance().AutoActivate();
            builder.RegisterType<SwapMessageSubscriber>().As<IStartable>().SingleInstance().AutoActivate();
            builder.RegisterType<HedgeTradeMessageSubscriber>().As<IStartable>().SingleInstance().AutoActivate();
        }

        private static void RegisterServiceBus(ContainerBuilder builder)
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
            builder.RegisterMyServiceBusSubscriberSingle<HedgeOperation>(serviceBusClient,
                HedgeOperation.TopicName,
                $"TradingPortfolio",
                TopicQueueType.PermanentWithSingleConnection);

            //Publishers
            builder.RegisterMyServiceBusPublisher<PortfolioTrade>(serviceBusClient, PortfolioTrade.TopicName, true);
            builder.RegisterMyServiceBusPublisher<PortfolioChangeBalance>(serviceBusClient,
                PortfolioChangeBalance.TopicName, true);
            builder.RegisterMyServiceBusPublisher<PortfolioSettlement>(serviceBusClient, PortfolioSettlement.TopicName,
                true);
            builder.RegisterMyServiceBusPublisher<PortfolioFeeShare>(serviceBusClient, PortfolioFeeShare.TopicName,
                true);
            builder.RegisterMyServiceBusPublisher<Portfolio>(serviceBusClient, Portfolio.TopicName, true);
        }
    }
}