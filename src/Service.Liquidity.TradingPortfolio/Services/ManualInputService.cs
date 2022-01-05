using Microsoft.Extensions.Logging;
using Service.IndexPrices.Client;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.Liquidity.TradingPortfolio.Grpc;
using Service.Liquidity.TradingPortfolio.Grpc.Models;
using System;
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
            try
            {
                _portfolioWalletManager.AddExternalWallet(request.WalletId, request.BrokerId, request.Source);
                return Task.FromResult(new WalletResponse()
                {
                    ErrorMessage = string.Empty,
                    Success = true
                });
            }
            catch (Exception e)
            {
                return Task.FromResult(new WalletResponse()
                {
                    ErrorMessage = e.Message,
                    Success = false
                });
            }
        }

        public Task<WalletResponse> AddInternalWalletAsync(WalletAddRequest request)
        {
            try
            {
                _portfolioWalletManager.AddInternalWallet(request.WalletId, request.BrokerId, request.WalletName);
                return Task.FromResult(new WalletResponse()
                {
                    ErrorMessage = string.Empty,
                    Success = true
                });
            }
            catch (Exception e)
            {
                return Task.FromResult(new WalletResponse()
                {
                    ErrorMessage = e.Message,
                    Success = false
                });
            }
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
