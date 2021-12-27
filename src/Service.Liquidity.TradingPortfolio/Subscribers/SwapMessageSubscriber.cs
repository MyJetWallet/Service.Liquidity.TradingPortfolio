﻿using DotNetCoreDecorators;
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
            await _manager.ApplyItemsAsync(ToItems(messages));
            return ;
        }

        private IReadOnlyList<PortfolioInputModel> ToItems(IReadOnlyList<SwapMessage> messages)
        { 
            var items = messages
                 .Select(i => new PortfolioInputModel
                 {
                     From = new InputModel()
                     { 
                         AssetId = i.AssetId1,
                         Volume = Convert.ToDecimal(i.Volume1),
                         WalletId = i.WalletId1,
                     },
                     To = new InputModel()
                     {
                         AssetId = i.AssetId2,
                         Volume = Convert.ToDecimal(i.Volume2),
                         WalletId = i.WalletId2,
                     }
                 }).ToList();

            return items;
        }
    }
}
