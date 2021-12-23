using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Liquidity.TradingPortfolio.Settings
{
    public class SettingsModel
    {
        [YamlProperty("Liquidity.TradingPortfolio.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("Liquidity.TradingPortfolio.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("Liquidity.TradingPortfolio.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
    }
}
