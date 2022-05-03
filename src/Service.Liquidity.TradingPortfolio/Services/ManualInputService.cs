using Microsoft.Extensions.Logging;
using Service.Liquidity.TradingPortfolio.Grpc;
using Service.Liquidity.TradingPortfolio.Grpc.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using MyJetWallet.Domain.Orders;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using Service.Liquidity.TradingPortfolio.Cache;
using Service.Liquidity.TradingPortfolio.Domain.Interfaces;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Services
{
    public class ManualInputService : IManualInputService
    {
        private readonly IPortfolioWalletManager _portfolioWalletManager;
        private readonly IPortfolioManager _portfolioManager;
        private readonly ILogger<ManualInputService> _logger;
        private readonly IManualTradeCacheStorage _manualTradeCacheStorage;

        public ManualInputService(
            IPortfolioWalletManager portfolioWalletManager,
            ILogger<ManualInputService> logger, 
            IPortfolioManager portfolioManager,
            IManualTradeCacheStorage manualTradeCacheStorage)
        {
            _portfolioWalletManager = portfolioWalletManager;
            _logger = logger;
            _portfolioManager = portfolioManager;
            _manualTradeCacheStorage = manualTradeCacheStorage;
        }

        public Task<WalletResponse> AddExternalWalletAsync(WalletAddRequest request)
        {
            try
            {
                _portfolioWalletManager.AddExternalAsync(request.WalletName, request.BrokerId, request.Source);
                
                return Task.FromResult(new WalletResponse
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
                
                return new SettlementResponse
                {
                    Success = true,
                    ErrorMessage = string.Empty
                };

            }
            catch (Exception e)
            {
                return new SettlementResponse
                {
                    Success = false,
                    ErrorMessage = $"Can't set new settlement by user {request.User}. {e.Message}"
                };
            }
        }

        public async Task<WalletResponse> AddInternalWalletAsync(WalletAddRequest request)
        {
            try
            {   await _portfolioWalletManager.AddInternalAsync(request.WalletId, request.BrokerId, request.WalletName);
                
                return new WalletResponse
                {
                    ErrorMessage = string.Empty,
                    Success = true
                };
            }
            catch (Exception e)
            {
                return new WalletResponse
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
                await _portfolioWalletManager.DeleteExternalByNameAsync(request.WalletId);
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
                await _portfolioWalletManager.DeleteInternalByNameAsync(request.WalletId);
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

        public Task<PortfolioResponse> GetPortfolioAsync()
        {
            return Task.FromResult(new PortfolioResponse
            {
                Portfolio = _portfolioManager.GetCurrentPortfolio()
            });
        }

        public Task<GetWalletsResponse> GetWalletsAsync()
        {
            return Task.FromResult(new GetWalletsResponse()
            {
                Wallets = _portfolioWalletManager.Get()
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

                return new BalanceResponse { ErrorMessage = string.Empty, Success = true };
            }
            catch (Exception e)
            {
                return new BalanceResponse { ErrorMessage = e.Message, Success = false };
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
            try
            {
                using var activity = MyTelemetry.StartActivity("CreateManualTradeAsync");
                request.AddToActivityAsJsonTag("CreateTradeManualRequest");
                var requestId = request.Id ?? Guid.NewGuid().ToString("N");
                var orderSide = request.Side == OrderSide.UnknownOrderSide ? request.Side :
                    (request.BaseVolume < 0 ? OrderSide.Sell : OrderSide.Buy);

                var lastResponse = _manualTradeCacheStorage.Get(requestId);
                if (lastResponse?.Response != null)
                {
                    _logger.LogWarning("CreateManualTradeAsync receive double request: {@request}", request);
                    return lastResponse.Response;
                }

                _logger.LogInformation("CreateManualTradeAsync receive request: {@request}", request);
                
                
                if (!request.IsValid(out var message))
                {
                    var responseInvalid = new ManualTradeResponse {Success = false, ErrorMessage = message};
                    _manualTradeCacheStorage.Add(requestId, new ManualTradeCacheElement
                    {
                        Response = responseInvalid,
                        Date = DateTime.UtcNow,

                    });
                    return responseInvalid;
                }
                
                var trade = new TradeMessage
                {
                    Id = requestId,
                    ReferenceId = string.Empty,
                    Market = request.AssociateSymbol,
                    Side = orderSide,
                    Price = request.Price,
                    Volume = Math.Abs(request.BaseVolume),
                    OppositeVolume = Math.Abs(request.QuoteVolume),
                    Timestamp = DateTime.UtcNow,
                    AssociateWalletId = request.WalletName,
                    AssociateBrokerId = request.BrokerId,
                    AssociateClientId = string.Empty,
                    AssociateSymbol = request.AssociateSymbol,
                    Source = "Manual",
                    BaseAsset = request.BaseAsset,
                    QuoteAsset = request.QuoteAsset,
                    Comment = request.Comment,
                    User = request.User,
                    FeeAsset = request.FeeAsset,
                    FeeVolume = Math.Abs(request.FeeVolume),
                    Type = PortfolioTradeType.Manual,
                };

                await _portfolioManager.ApplyTradeAsync(trade);
                var response = new ManualTradeResponse { Success = true };

                _logger.LogInformation("CreateManualTradeAsync return response: {@response}", response);
                _manualTradeCacheStorage.Add(request.Id, new ManualTradeCacheElement
                {
                    Response = response,
                    Date = DateTime.UtcNow,

                });
                _manualTradeCacheStorage.CleanUp();
                return response;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Creating failed: {JsonConvert.SerializeObject(exception)}");
                return new ManualTradeResponse { Success = false, ErrorMessage = exception.Message };
            }
        }
    }
}
