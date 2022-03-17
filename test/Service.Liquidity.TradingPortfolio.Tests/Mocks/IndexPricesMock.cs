using System;
using System.Collections.Generic;
using System.Linq;
using Service.IndexPrices.Client;
using Service.IndexPrices.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests.Mocks;

public class IndexPricesMock : IIndexPricesClient
{
    public Dictionary<string, IndexPrice> Prices = new Dictionary<string, IndexPrice>();
    public void Set(string asset, decimal price)
    { 
        if(!Prices.TryGetValue(asset, out var item))
        {
            item = new IndexPrice()
            {
                Asset = asset,
                UpdateDate = DateTime.UtcNow,
            };
        }
        item.UsdPrice = price;
        Prices[asset] = item;
    }

    public IndexPrice GetIndexPriceByAssetAsync(string asset)
    {
        if (Prices.TryGetValue(asset, out var item))
        {
            return item;
        }
        return null;
    }

    public (IndexPrice, decimal) GetIndexPriceByAssetVolumeAsync(string asset, decimal volume)
    {
        var item = GetIndexPriceByAssetAsync(asset);

        if (item == null)
            return (new IndexPrice
            {
                Asset = asset,
                UpdateDate = DateTime.UtcNow,
                UsdPrice = 0m
            }, 0m);

        return (item, item.UsdPrice * volume);
    }

    public List<IndexPrice> GetIndexPricesAsync()
    {
        return Prices.Values.ToList();
    }
}