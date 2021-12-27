namespace Service.Liquidity.TradingPortfolio.Domain.Models
{
    public class PortfolioWallet
    {
        public bool IsInternal { get; set; }
        public string Id { get; set; } 
        public string InternalWalletId { get; set; } 
        public string ExternalSource { get; set; } 
    }
}
