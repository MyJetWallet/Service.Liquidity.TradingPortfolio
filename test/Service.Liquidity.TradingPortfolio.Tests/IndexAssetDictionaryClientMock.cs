using System;
using System.Collections.Generic;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests;

public class IndexAssetDictionaryClientMock : IIndexAssetDictionaryClient
{
    public Dictionary<string, IndexAsset> Data { get; set; } = new();

    public IndexAsset GetIndexAsset(string broker, string symbol)
    {
        if(Data.TryGetValue(symbol, out var index))
            return index;

        return null;
    }

    public IReadOnlyList<IndexAsset> GetAll()
    {
        throw new NotImplementedException();
    }
}