using System.Threading.Tasks;
using Autofac;
using DotNetCoreDecorators;
using Service.Liquidity.Monitoring.Domain.Models.Hedging;
using Service.Liquidity.TradingPortfolio.Domain;

namespace Service.Liquidity.TradingPortfolio.Subscribers
{
    public class HedgeTradeMessageSubscriber : IStartable
    {
        private readonly ISubscriber<HedgeTradeMessage> _subscriber;
        private readonly IPortfolioManager _manager;

        public HedgeTradeMessageSubscriber(
            ISubscriber<HedgeTradeMessage> subscriber,
            IPortfolioManager manager
        )
        {
            _subscriber = subscriber;
            _manager = manager;
        }

        public void Start()
        {
            _subscriber.Subscribe(Handle);
        }

        private async ValueTask Handle(HedgeTradeMessage message)
        {
            await _manager.ApplyHedgeTradeAsync(message);
        }
    }
}