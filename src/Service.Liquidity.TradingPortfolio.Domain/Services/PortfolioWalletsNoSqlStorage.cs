using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.TradingPortfolio.Domain.Interfaces;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;

namespace Service.Liquidity.TradingPortfolio.Domain.Services;

public class PortfolioWalletsNoSqlStorage : IPortfolioWalletsStorage
{
    private readonly IMyNoSqlServerDataWriter<PortfolioWalletNoSql> _myNoSqlServerDataWriter;

    public PortfolioWalletsNoSqlStorage(
        IMyNoSqlServerDataWriter<PortfolioWalletNoSql> myNoSqlServerDataWriter
    )
    {
        _myNoSqlServerDataWriter = myNoSqlServerDataWriter;
    }

    public async Task AddOrUpdateAsync(PortfolioWallet model)
    {
        var nosqlModel = PortfolioWalletNoSql.Create(model);
        await _myNoSqlServerDataWriter.InsertOrReplaceAsync(nosqlModel);
    }

    public async Task<IEnumerable<PortfolioWallet>> GetAsync()
    {
        var models = await _myNoSqlServerDataWriter.GetAsync();

        return models.Select(m => m.Wallet);
    }

    public async Task<PortfolioWallet> GetAsync(string id)
    {
        var model = await _myNoSqlServerDataWriter.GetAsync(PortfolioWalletNoSql.GeneratePartitionKey(),
            PortfolioWalletNoSql.GenerateRowKey(id));

        return model.Wallet;
    }

    public async Task DeleteAsync(string id)
    {
        await _myNoSqlServerDataWriter.DeleteAsync(PortfolioWalletNoSql.GeneratePartitionKey(),
            PortfolioWalletNoSql.GenerateRowKey(id));
    }

    public async Task BulkInsetOrUpdateAsync(IEnumerable<PortfolioWallet> models)
    {
        var nosqlModels = models.Select(PortfolioWalletNoSql.Create);
        await _myNoSqlServerDataWriter.BulkInsertOrReplaceAsync(nosqlModels);
    }
}