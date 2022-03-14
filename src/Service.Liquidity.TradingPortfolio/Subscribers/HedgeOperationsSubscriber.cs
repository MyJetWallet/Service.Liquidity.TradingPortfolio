using System.Threading.Tasks;
using Autofac;
using DotNetCoreDecorators;
using Service.Liquidity.Hedger.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.Liquidity.TradingPortfolio.Domain.Interfaces;

namespace Service.Liquidity.TradingPortfolio.Subscribers
{
    public class HedgeOperationsSubscriber : IStartable
    {
        private readonly ISubscriber<HedgeOperation> _subscriber;
        private readonly IPortfolioManager _manager;

        public HedgeOperationsSubscriber(
            ISubscriber<HedgeOperation> subscriber,
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

        private async ValueTask Handle(HedgeOperation operation)
        {
            await _manager.ApplyHedgeOperationAsync(operation);
        }
    }
}