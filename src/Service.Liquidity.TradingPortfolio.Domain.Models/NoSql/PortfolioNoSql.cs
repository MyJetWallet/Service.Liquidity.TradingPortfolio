using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.TradingPortfolio.Domain.Models.NoSql
{
    public class PortfolioNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-liquitity-tradingportfolio-portfolio";
        public static string GeneratePartitionKey() => "Portfolio";
        public static string GenerateRowKey() => "Portfolio";

        public Portfolio Portfolio { get; set; }

        public static PortfolioNoSql Create(Portfolio portfolio) =>
            new PortfolioNoSql()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(),
                Portfolio = portfolio
            };
    }
}
