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

        public Task<WalletResponse> AddExternalWalletAsync(WalletAddRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<WalletResponse> AddInternalWalletAsync(WalletAddRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<WalletResponse> DeleteExternalWalletAsync(SetVelocityRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<WalletResponse> DeleteInternalWalletAsync(WalletDeleteRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<PortfolioResponse> GetPortfolioAsync(PortfolioRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<SetBalanceResponse> SetBalanceAsync(SetBalanceRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<SetVelocityResponse> SetVelocityAsync(SetVelocityRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
