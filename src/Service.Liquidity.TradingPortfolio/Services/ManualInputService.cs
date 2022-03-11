using Microsoft.Extensions.Logging;
using Service.IndexPrices.Client;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.Liquidity.TradingPortfolio.Grpc;
using Service.Liquidity.TradingPortfolio.Grpc.Models;
using System;
using System.Threading.Tasks;
using MyJetWallet.Domain.Orders;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Services
{
    public class ManualInputService : IManualInputService
    {
        private readonly IPortfolioWalletManager _portfolioWalletManager;
        private readonly IPortfolioManager _portfolioManager;
        private readonly ILogger<ManualInputService> _logger;

        public ManualInputService(IPortfolioWalletManager portfolioWalletManager,
            ILogger<ManualInputService> logger, 
            IPortfolioManager portfolioManager)
        {
            _portfolioWalletManager = portfolioWalletManager;
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
                await _portfolioManager.SetManualSettlementAsync(new PortfolioSettlement
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
                    ErrorMessage = $"Can't set new settlement by user {request.User}"
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
                await _portfolioManager.SetVelocityLowHighAsync(
                    request.Asset,
                    request.VelocityLowOpen,
                    request.VelocityHighOpen);

                return new SetVelocityResponse() { ErrorMessage = string.Empty, Success = true };
            }
            catch (Exception e)
            {
                return new SetVelocityResponse() { ErrorMessage = e.Message, Success = false };
            }
        }
        
        public async Task<ManualTradeResponse> ReportManualTradeAsync(ReportManualTradeRequest request)
        {
            using var activity = MyTelemetry.StartActivity("CreateManualTradeAsync");

            request.AddToActivityAsJsonTag("CreateTradeManualRequest");
            
            _logger.LogInformation($"CreateManualTradeAsync receive request: {JsonConvert.SerializeObject(request)}");
            
            if (string.IsNullOrWhiteSpace(request.BrokerId) ||
                string.IsNullOrWhiteSpace(request.WalletName) ||
                string.IsNullOrWhiteSpace(request.AssociateSymbol) ||
                string.IsNullOrWhiteSpace(request.BaseAsset) ||
                string.IsNullOrWhiteSpace(request.QuoteAsset) ||
                string.IsNullOrWhiteSpace(request.Comment) ||
                string.IsNullOrWhiteSpace(request.User) ||
                request.Price == 0 ||
                request.BaseVolume == 0 ||
                request.QuoteVolume == 0 ||
                (request.BaseVolume > 0 && request.QuoteVolume > 0) ||
                (request.BaseVolume < 0 && request.QuoteVolume < 0))
            {
                _logger.LogError($"Bad request entity: {JsonConvert.SerializeObject(request)}");
                return new ManualTradeResponse() {Success = false, ErrorMessage = "Incorrect entity"};
            }
            
            var trade = new TradeMessage()
            {
                Id = Guid.NewGuid().ToString("N"),
                ReferenceId = string.Empty,
                Market = request.AssociateSymbol,
                Side = request.BaseVolume < 0 ? OrderSide.Sell : OrderSide.Buy,
                Price = request.Price,
                Volume = request.BaseVolume,
                OppositeVolume = request.QuoteVolume,
                Timestamp = DateTime.UtcNow,
                AssociateWalletId = request.WalletName,
                AssociateBrokerId = request.BrokerId,
                AssociateClientId = string.Empty,
                AssociateSymbol = request.AssociateSymbol,
                Source = "manual",
                BaseAsset = request.BaseAsset,
                QuoteAsset = request.QuoteAsset,
                Comment = request.Comment,
                User = request.User,
                FeeAsset = request.FeeAsset,
                FeeVolume = request.FeeVolume
            };     
            try
            {
                await _portfolioManager.ApplyTradeAsync(trade);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Creating failed: {JsonConvert.SerializeObject(exception)}");
                return new ManualTradeResponse() {Success = false, ErrorMessage = exception.Message};
            }

            var response = new ManualTradeResponse() {Success = true};
            
            _logger.LogInformation($"CreateManualTradeAsync return reponse: {JsonConvert.SerializeObject(response)}");
            return response;
        }        
    }
}
