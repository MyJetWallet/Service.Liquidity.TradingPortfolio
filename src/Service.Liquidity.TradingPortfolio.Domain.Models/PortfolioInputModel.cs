using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Domain.Models
{
    public class InputModel
    {
        public string WalletId { get;set;}
        public string AssetId { get; set; }
        public decimal Volume { get; set; }
    }
    public class PortfolioInputModel
    {
        public InputModel From { get; set; }
        public InputModel To { get; set; }
    }
}
