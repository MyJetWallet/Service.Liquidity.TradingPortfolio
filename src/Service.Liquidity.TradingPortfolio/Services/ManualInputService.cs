using Microsoft.Extensions.Logging;
using Service.IndexPrices.Client;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.Liquidity.TradingPortfolio.Grpc;
using Service.Liquidity.TradingPortfolio.Grpc.Models;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Services
{
    public class ManualInputService : IManualInputService
    {
        private readonly IPortfolioWalletManager _portfolioWalletManager;
        private readonly IIndexPricesClient _indexPricesClient;
        private readonly ILogger<ManualInputService> _logger;

        public ManualInputService(IPortfolioWalletManager portfolioWalletManager,
            IIndexPricesClient indexPricesClient, 
            ILogger<ManualInputService> logger)
        {
            _portfolioWalletManager = portfolioWalletManager;
            _indexPricesClient = indexPricesClient;
            _logger = logger;
        }


        public Task<DailyVelocityResponse> SetDailyVelocityAsync(DailyVelocityRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
