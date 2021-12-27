using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;

namespace Service.Liquidity.TradingPortfolio.Client
{
    [UsedImplicitly]
    public class LiquidityTradingPortfolioClientFactory: MyGrpcClientFactory
    {
        public LiquidityTradingPortfolioClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        //public IHelloService GetHelloService() => CreateGrpcService<IHelloService>();
    }
}
