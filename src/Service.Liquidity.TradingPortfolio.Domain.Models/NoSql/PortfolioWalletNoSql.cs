using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.TradingPortfolio.Domain.Models.NoSql
{
    public class PortfolioWalletNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquitity-tradingportfolio";
        public static string GeneratePartitionKey() => "PortfolioWallet";
        public static string GenerateRowKey(string walletRowId) =>
            $"{walletRowId}";

        public PortfolioWallet Wallet { get; set; }

        public static PortfolioWalletNoSql Create(PortfolioWallet wallet) =>
            new PortfolioWalletNoSql()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(wallet.Id),
                Wallet = wallet,
            };
    }
}
