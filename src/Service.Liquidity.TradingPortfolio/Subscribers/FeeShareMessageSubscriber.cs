using DotNetCoreDecorators;
using System.Threading.Tasks;
using Autofac;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.FeeShareEngine.Domain.Models.Models;

namespace Service.Liquidity.TradingPortfolio.Subscribers
{
    public class FeeShareMessageSubscriber : IStartable
    {
        private readonly ISubscriber<FeeShareEntity> _subscriber;
        private readonly IPortfolioManager _manager;

        public FeeShareMessageSubscriber(
            ISubscriber<FeeShareEntity> subscriber,
            IPortfolioManager manager
        )
        {
            _subscriber = subscriber;
            _manager = manager;
        }

        public void Start()
        {
            _subscriber.Subscribe(Handler);
        }

        private async ValueTask Handler(FeeShareEntity message)
        {
            await _manager.ApplyFeeShareAsync(message);
        }
    }
}