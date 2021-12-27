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
            await _manager.ApplyItemsAsync(ToItems(messages));
            return ;
        }

        private IReadOnlyList<PortfolioInputModel> ToItems(IReadOnlyList<TradeMessage> messages)
        {
            var items = messages
                 .Select(i => new PortfolioInputModel
                 {
                     From = new InputModel()
                     {
                         AssetId = i.BaseAsset,
                         Volume = i.Volume,
                         WalletId = i.AssociateWalletId,
                     },
                     To = new InputModel()
                     {
                         AssetId = i.QuoteAsset,
                         Volume = i.OppositeVolume,
                         WalletId = i.AssociateBrokerId,
                     }
                 }).ToList();

            return items;
        }
    }
}
