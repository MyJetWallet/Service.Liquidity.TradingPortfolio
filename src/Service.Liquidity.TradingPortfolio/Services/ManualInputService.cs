using Microsoft.Extensions.Logging;
using Service.IndexPrices.Client;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.Liquidity.TradingPortfolio.Grpc;
using Service.Liquidity.TradingPortfolio.Grpc.Models;
using System;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Services
{
    public class ManualInputService : IManualInputService
    {
        private readonly IPortfolioWalletManager _portfolioWalletManager;
        private readonly IPortfolioManager _portfolioManager;
        private readonly IIndexPricesClient _indexPricesClient;
        private readonly ILogger<ManualInputService> _logger;

        public ManualInputService(IPortfolioWalletManager portfolioWalletManager,
            IIndexPricesClient indexPricesClient,
            ILogger<ManualInputService> logger, 
            IPortfolioManager portfolioManager)
        {
            _portfolioWalletManager = portfolioWalletManager;
            _indexPricesClient = indexPricesClient;
            _logger = logger;
            _portfolioManager = portfolioManager;
        }

        public Task<WalletResponse> AddExternalWalletAsync(WalletAddRequest request)
        {
            try
            {
                _portfolioWalletManager.AddExternalWallet(request.WalletName, request.BrokerId, request.Source);
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

        public async Task<SettlementResponse> SetSettlementAsync(SettlementRequest request)
        {
            try
            {
                await _portfolioManager.SetManualSettelmentAsync(new PortfolioSettlement
                {
                    BrokerId = request.BrokerId,
                    User = request.User,
                    SettlementDate = DateTime.UtcNow,
                    Asset = request.Asset,
                    VolumeFrom = request.VolumeFrom,
                    VolumeTo = request.VolumeTo,
                    Comment = request.Comment,
                    WalletTo = request.WalletTo,
                    WalletFrom = request.WalletFrom,

                });
                return new SettlementResponse()
                {
                    Success = true,
                    ErrorMessage = string.Empty
                };

            }
            catch (Exception e)
            {
                return new SettlementResponse()
                {
                    Success = false,
                    ErrorMessage = $"Can't set new settelment by user {request.User}"
                };
            }
        }

        public async Task<WalletResponse> AddInternalWalletAsync(WalletAddRequest request)
        {
            try
            {   await _portfolioWalletManager.AddInternalWallet(request.WalletId, request.BrokerId, request.WalletName);
                return new WalletResponse()
                {
                    ErrorMessage = string.Empty,
                    Success = true
                };
            }
            catch (Exception e)
            {
                return new WalletResponse()
                {
                    ErrorMessage = e.Message,
                    Success = false
                };
            }
        }

        public async Task<WalletResponse> DeleteExternalWalletAsync(WalletDeleteRequest request)
        {
            try
            {
                await _portfolioWalletManager.DeleteExternalWalletByWalletName(request.WalletId);
                return new WalletResponse()
                {
                    ErrorMessage = string.Empty,
                    Success = true
                };
            }
            catch (Exception e)
            {
                return new WalletResponse()
                {
                    ErrorMessage = e.Message,
                    Success = false
                };
            }
        }

        public async Task<WalletResponse> DeleteInternalWalletAsync(WalletDeleteRequest request)
        {
            try
            {
                await _portfolioWalletManager.DeleteInternalWalletByWalletName(request.WalletId);
                return new WalletResponse()
                {
                    ErrorMessage = string.Empty,
                    Success = true
                };
            }
            catch (Exception e)
            {
                return new WalletResponse()
                {
                    ErrorMessage = e.Message,
                    Success = false
                };
            }
        }

        public async Task<PortfolioResponse> GetPortfolioAsync()
        {
            return new PortfolioResponse()
            {
                Portfolio = _portfolioManager.GetCurrentPortfolio()
            };
        }

        public Task<GetWalletsResponse> GetWalletsAsync()
        {
            return Task.FromResult(new GetWalletsResponse()
            {
                Wallets = _portfolioWalletManager.GetWallets()
            });

        }

        public async Task<BalanceResponse> SetBalanceAsync(BalanceRequest request)
        {
            try
            {
                await _portfolioManager.SetManualBalanceAsync(request.Wallet,
                  request.Asset,
                  request.Balance,
                  request.Comment,
                  request.User);

                return new BalanceResponse() { ErrorMessage = string.Empty, Success = true };
            }
            catch (Exception e)
            {
                return new BalanceResponse() { ErrorMessage = e.Message, Success = false };
            }
        }

        public async Task<SetVelocityResponse> SetVelocityAsync(SetVelocityRequest request)
        {
            try
            {
                await _portfolioManager.SetDailyVelocityAsync(
                    request.Asset,
                    request.Velocity);

                return new SetVelocityResponse() { ErrorMessage = string.Empty, Success = true };
            }
            catch (Exception e)
            {
                return new SetVelocityResponse() { ErrorMessage = e.Message, Success = false };
            }
        }
    }
}
