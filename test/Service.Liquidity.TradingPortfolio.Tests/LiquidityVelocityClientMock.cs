using System;
using System.Collections.Generic;
using Service.Liquidity.Velocity.Client;
using Service.Liquidity.Velocity.Domain.Models.NoSql;

namespace Service.Liquidity.TradingPortfolio.Tests;

public class LiquidityVelocityClientMock : ILiquidityVelocityClient
{
    public VelocityNoSql GetVelocityByAsset(string brokerId, string asset)
    {
        var velocity = VelocityNoSql.Create(brokerId, new Velocity.Domain.Models.Velocity
        {
            Asset = asset,
            LowOpenAverage = 0.5m,
            HighOpenAverage = 1m,
            CalcDate = DateTime.UtcNow
        });
        return velocity;
    }

    public IReadOnlyList<VelocityNoSql> GetAllAssets()
    {
        throw new NotImplementedException();
    }
}