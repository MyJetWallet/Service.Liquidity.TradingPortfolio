using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Liquidity.TradingPortfolio.Settings
{
    public class SettingsModel
    {
        [YamlProperty("LiquidityTradingPortfolio.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("LiquidityTradingPortfolio.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("LiquidityTradingPortfolio.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("LiquidityTradingPortfolio.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("LiquidityTradingPortfolio.BaseCurrencyConverterGrpcServiceUrl")]
        public string BaseCurrencyConverterGrpcServiceUrl { get; set; }

        [YamlProperty("LiquidityPortfolio.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("LiquidityTradingPortfolio.ServiceBusQuerySuffix")]
        public string ServiceBusQuerySuffix { get; set; }
    }
}
