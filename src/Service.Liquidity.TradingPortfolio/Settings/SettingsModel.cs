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
    }
}
