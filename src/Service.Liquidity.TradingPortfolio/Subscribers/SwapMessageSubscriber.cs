using DotNetCoreDecorators;
using Service.Liquidity.Converter.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain;
using Autofac;
using Service.Liquidity.TradingPortfolio.Domain.Interfaces;

namespace Service.Liquidity.TradingPortfolio.Subscribers
{
    public class SwapMessageSubscriber : IStartable
    {
        private readonly ISubscriber<IReadOnlyList<SwapMessage>> _subscriber;
        private readonly IPortfolioManager _manager;

        public SwapMessageSubscriber(
            ISubscriber<IReadOnlyList<SwapMessage>> subscriber,
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

        private async ValueTask Handler(IReadOnlyList<SwapMessage> messages)
        {
            await _manager.ApplySwapsAsync(messages);
        }
    }
}