using System;
using Service.Liquidity.TradingPortfolio.Grpc.Models;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Grpc
{
    [ServiceContract]
    public interface IManualInputService
    {
        #region Manual Settings
        [OperationContract]
        Task<SetVelocityResponse> SetVelocityAsync(SetVelocityRequest request);
        
        [OperationContract]
        Task<BalanceResponse> SetBalanceAsync(BalanceRequest request);

        [OperationContract]
        Task <SettlementResponse> SetSettlementAsync(SettlementRequest request);

        #endregion

        #region Wallets CRUD
        [OperationContract]
        Task<WalletResponse> AddInternalWalletAsync(WalletAddRequest request);
        
        [OperationContract]
        Task<WalletResponse> DeleteInternalWalletAsync(WalletDeleteRequest request);
        
        [OperationContract]
        Task<WalletResponse> AddExternalWalletAsync(WalletAddRequest request);
        
        [OperationContract]
        Task<WalletResponse> DeleteExternalWalletAsync(WalletDeleteRequest request);
        
        [OperationContract]
        Task<GetWalletsResponse> GetWalletsAsync();
        #endregion
        
        #region Portfolio
        [OperationContract]
        Task<PortfolioResponse> GetPortfolioAsync();
        #endregion
        
        [OperationContract]
        Task<ManualTradeResponse> ReportManualTradeAsync(ReportManualTradeRequest request);
    }
}
