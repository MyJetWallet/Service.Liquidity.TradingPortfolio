using DotNetCoreDecorators;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.FeeShareEngine.Domain.Models.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Subscribers
{
    public class FeeShareMessageSubscriber
    {
        private readonly IPortfolioManager _manager;

        public FeeShareMessageSubscriber(ISubscriber<FeeShareEntity> subscriber, 
            IPortfolioManager manager)
        {
            subscriber.Subscribe(Handler);
            _manager = manager;
        }

        private async ValueTask Handler(FeeShareEntity message)
        {
            await _manager.ApplyItemAsync(ToItem(message));
            return ;
        }

        private PortfolioInputModel ToItem(FeeShareEntity message)
        {
            var item = new PortfolioInputModel
            {
                From = new InputModel()
                {
                    AssetId = message.BrokerId,
                    Volume = message.FeeShareAmountInTargetAsset,
                    WalletId = message.ConverterWalletId,
                },
                To = new InputModel()
                {
                    AssetId = message.FeeAsset,
                    Volume = message.FeeShareAmountInTargetAsset,
                    WalletId = message.FeeShareWalletId,
                }
            };

            return item;
        }
    }
}
