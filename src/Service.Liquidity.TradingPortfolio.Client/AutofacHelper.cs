using Autofac;
using Service.Liquidity.TradingPortfolio.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.Liquidity.TradingPortfolio.Client
{
    public static class AutofacHelper
    {
        public static void RegisterLiquidityTradingPortfolioClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new LiquidityTradingPortfolioClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetHelloService()).As<IHelloService>().SingleInstance();
        }
    }
}
