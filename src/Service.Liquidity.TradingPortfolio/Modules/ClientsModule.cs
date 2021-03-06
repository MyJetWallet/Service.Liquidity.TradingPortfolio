using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.AssetsDictionary.Client;
using Service.IndexPrices.Client;

namespace Service.Liquidity.TradingPortfolio.Modules
{
    public class ClientsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.Settings.MyNoSqlReaderHostPort, Program.LogFactory);
            builder.RegisterIndexAssetClients(myNoSqlClient);
            builder.RegisterIndexPricesClient(myNoSqlClient);
        }
    }
}