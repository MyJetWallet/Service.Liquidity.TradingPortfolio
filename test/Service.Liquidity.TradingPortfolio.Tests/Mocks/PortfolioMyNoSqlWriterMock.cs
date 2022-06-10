using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;

namespace Service.Liquidity.TradingPortfolio.Tests.Mocks;

public class PortfolioMyNoSqlWriterMock : IMyNoSqlServerDataWriter<PortfolioNoSql>
{
    private Portfolio _portfolio = new()
    {
        Assets = new Dictionary<string, Portfolio.Asset>()
    };

    public ValueTask InsertAsync(PortfolioNoSql entity)
    {
        _portfolio = entity.Portfolio;
        
        return ValueTask.CompletedTask;
    }

    public ValueTask InsertOrReplaceAsync(PortfolioNoSql entity)
    {
        _portfolio = entity.Portfolio;
        
        return ValueTask.CompletedTask;
    }

    public ValueTask CleanAndKeepLastRecordsAsync(string partitionKey, int amount)
    {
        throw new NotImplementedException();
    }

    public ValueTask BulkInsertOrReplaceAsync(IReadOnlyList<PortfolioNoSql> entity, DataSynchronizationPeriod dataSynchronizationPeriod = DataSynchronizationPeriod.Sec5)
    {
        throw new NotImplementedException();
    }

    public ValueTask CleanAndBulkInsertAsync(IReadOnlyList<PortfolioNoSql> entity, DataSynchronizationPeriod dataSynchronizationPeriod = DataSynchronizationPeriod.Sec5)
    {
        throw new NotImplementedException();
    }

    public ValueTask CleanAndBulkInsertAsync(string partitionKey, IReadOnlyList<PortfolioNoSql> entity,
        DataSynchronizationPeriod dataSynchronizationPeriod = DataSynchronizationPeriod.Sec5)
    {
        throw new NotImplementedException();
    }

    public ValueTask<OperationResult> ReplaceAsync(string partitionKey, string rowKey, Func<PortfolioNoSql, bool> updateCallback,
        DataSynchronizationPeriod syncPeriod = DataSynchronizationPeriod.Sec5)
    {
        throw new NotImplementedException();
    }

    public ValueTask<OperationResult> MergeAsync(string partitionKey, string rowKey, Func<PortfolioNoSql, bool> updateCallback,
        DataSynchronizationPeriod syncPeriod = DataSynchronizationPeriod.Sec5)
    {
        throw new NotImplementedException();
    }

    public ValueTask<List<PortfolioNoSql>> GetAsync()
    {
        _portfolio = new()
        {
            Assets = new Dictionary<string, Portfolio.Asset>()
        };

        return ValueTask.FromResult<List<PortfolioNoSql>>(new List<PortfolioNoSql>
            { PortfolioNoSql.Create(_portfolio) });
    }

    public IAsyncEnumerable<PortfolioNoSql> GetAllAsync(int bulkRecordsCount)
    {
        throw new NotImplementedException();
    }

    public ValueTask<List<PortfolioNoSql>> GetAsync(string partitionKey)
    {
        throw new NotImplementedException();
    }

    public ValueTask<PortfolioNoSql> GetAsync(string partitionKey, string rowKey)
    {
        throw new NotImplementedException();
    }

    public ValueTask<List<PortfolioNoSql>> GetMultipleRowKeysAsync(string partitionKey, IReadOnlyList<string> rowKeys)
    {
        throw new NotImplementedException();
    }

    public ValueTask<PortfolioNoSql> DeleteAsync(string partitionKey, string rowKey)
    {
        throw new NotImplementedException();
    }

    public ValueTask<List<PortfolioNoSql>> QueryAsync(string query)
    {
        throw new NotImplementedException();
    }

    public ValueTask<List<PortfolioNoSql>> GetHighestRowAndBelow(string partitionKey, string rowKeyFrom, int amount)
    {
        throw new NotImplementedException();
    }

    public ValueTask CleanAndKeepMaxPartitions(int maxAmount)
    {
        throw new NotImplementedException();
    }

    public ValueTask CleanAndKeepMaxRecords(string partitionKey, int maxAmount)
    {
        throw new NotImplementedException();
    }

    public ValueTask<int> GetCountAsync(string partitionKey)
    {
        throw new NotImplementedException();
    }

    public ValueTask<ITransactionsBuilder<PortfolioNoSql>> BeginTransactionAsync()
    {
        throw new NotImplementedException();
    }
}