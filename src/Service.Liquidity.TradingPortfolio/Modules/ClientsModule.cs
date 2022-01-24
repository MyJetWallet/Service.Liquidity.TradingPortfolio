using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.AssetsDictionary.Client;
using Service.IndexPrices.Client;
using Service.Liquidity.Velocity.Client;

namespace Service.Liquidity.TradingPortfolio.Modules
{
    public class ClientsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            builder.RegisterIndexAssetClients(myNoSqlClient);
            //builder.RegisterBaseCurrencyConverterClient(Program.Settings.BaseCurrencyConverterGrpcServiceUrl, myNoSqlClient);
            //builder.RegisterCurrentPricesClient(myNoSqlClient);
            // $ для каждого актива
            builder.RegisterIndexPricesClient(myNoSqlClient);
            builder.RegisterLiquidityVelocityClient(myNoSqlClient);
        }
    }
}
