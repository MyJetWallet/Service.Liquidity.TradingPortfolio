using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public interface IPortfolioWalletManager
    {
        PortfolioWallet GetInternalWalletByWalletId(string walletId);
    }
}
