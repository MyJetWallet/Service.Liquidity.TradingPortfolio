using System;
using MyJetWallet.Domain.Orders;
using Service.Liquidity.Hedger.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Domain.Utils;

public static class PortfolioTradeMappings
{
    public static PortfolioTrade ToPortfolioTrade(this HedgeTrade trade, string walletName, 
        decimal baseVolumeInUsd, decimal baseIndexPriceInUsd, 
        decimal quoteVolumeInUsd, decimal quoteIndexPriceInUsd)
    {
        return new()
        {
            TradeId = trade.Id,
            AssociateBrokerId = "jetwallet",
            BaseWalletName = walletName,
            QuoteWalletName = walletName,
            AssociateSymbol = trade.BaseAsset + "|" + trade.QuoteAsset,
            BaseAsset = trade.BaseAsset,
            QuoteAsset = trade.QuoteAsset,
            Side = OrderSide.Buy,
            Price = trade.Price,
            BaseVolume = trade.BaseVolume,
            QuoteVolume = trade.QuoteVolume,
            BaseVolumeInUsd = baseVolumeInUsd,
            QuoteVolumeInUsd = quoteVolumeInUsd,
            BaseAssetPriceInUsd = baseIndexPriceInUsd,
            QuoteAssetPriceInUsd = quoteIndexPriceInUsd,
            DateTime = DateTime.UtcNow,
            Source = "Hedger",
            Comment = "Hedge trade",
            FeeAsset = trade.FeeAsset,
            FeeVolume = trade.FeeVolume,
            User = ""
        };
    }
}