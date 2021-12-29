using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.Liquidity.TradingPortfolio.Grpc;

namespace Service.Liquidity.TradingPortfolio.Client
{
    [UsedImplicitly]
    public class LiquidityTradingPortfolioClientFactory: MyGrpcClientFactory
    {
        public LiquidityTradingPortfolioClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IManualInputService GetManualInputService() => CreateGrpcService<IManualInputService>();
    }
}
