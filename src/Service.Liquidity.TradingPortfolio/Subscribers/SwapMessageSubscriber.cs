using DotNetCoreDecorators;
using Service.Liquidity.Converter.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Linq;

namespace Service.Liquidity.TradingPortfolio.Subscribers
{
    public class SwapMessageSubscriber
    {
        private readonly IPortfolioManager _manager;

        public SwapMessageSubscriber(ISubscriber<IReadOnlyList<SwapMessage>> subscriber, 
            IPortfolioManager manager)
        {
            subscriber.Subscribe(Handler);
            this._manager = manager;
        }

        private async ValueTask Handler(IReadOnlyList<SwapMessage> messages)
        {
            await _manager.ApplySwapsAsync(messages);
            return ;
        }
    }
}
