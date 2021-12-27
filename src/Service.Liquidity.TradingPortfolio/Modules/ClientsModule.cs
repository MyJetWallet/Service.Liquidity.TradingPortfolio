using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.AssetsDictionary.Client;
using Service.BaseCurrencyConverter.Client;
using Service.IndexPrices.Client;

namespace Service.Liquidity.TradingPortfolio.Modules
{
    public class ClientsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            builder.RegisterAssetsDictionaryClients(myNoSqlClient);
            builder.RegisterBaseCurrencyConverterClient(Program.Settings.BaseCurrencyConverterGrpcServiceUrl, myNoSqlClient);
            builder.RegisterCurrentPricesClient(myNoSqlClient);
            // $ ��� ������� ������
            builder.RegisterIndexPricesClient(myNoSqlClient);
        }
    }
}
