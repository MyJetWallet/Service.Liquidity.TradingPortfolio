using DotNetCoreDecorators;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.Liquidity.PortfolioHedger.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Linq;

namespace Service.Liquidity.TradingPortfolio.Subscribers
{
    public class HadgeTradeMessageSubscriber
    {
        private readonly IPortfolioManager _manager;

        public HadgeTradeMessageSubscriber(ISubscriber<IReadOnlyList<TradeMessage>> subscriber, 
            IPortfolioManager manager)
        {
            subscriber.Subscribe(Handler);
            _manager = manager;
        }

        private async ValueTask Handler(IReadOnlyList<TradeMessage> messages)
        {
            await _manager.ApplyTradesAsync(messages);
        }
    }
}
