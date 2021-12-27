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

        [YamlProperty("LiquidityPortfolio.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("LiquidityPortfolio.BaseCurrencyConverterGrpcServiceUrl")]
        public string BaseCurrencyConverterGrpcServiceUrl { get; set; }
    }
}
