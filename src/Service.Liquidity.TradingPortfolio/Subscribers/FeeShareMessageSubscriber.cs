using DotNetCoreDecorators;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.FeeShareEngine.Domain.Models.Models;

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
            await _manager.ApplyFeeShareAsync(message);
        }
    }
}
