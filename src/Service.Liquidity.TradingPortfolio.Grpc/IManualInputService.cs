using Service.Liquidity.TradingPortfolio.Grpc.Models;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Grpc
{
    [ServiceContract]
    public interface IManualInputService
    {
        [OperationContract]
        #region Manual Settings
        Task<SetVelocityResponse> SetVelocityAsync(SetVelocityRequest request);
        Task<SetBalanceResponse> SetBalanceAsync(SetBalanceRequest request);
        #endregion
        #region Wallets CRUD
        Task<WalletResponse> AddInternalWalletAsync(WalletAddRequest request);
        Task<WalletResponse> DeleteInternalWalletAsync(WalletDeleteRequest request);
        Task<WalletResponse> AddExternalWalletAsync(WalletAddRequest request);
        Task<WalletResponse> DeleteExternalWalletAsync(SetVelocityRequest request);
        Task<GetWalletsResponse> GetWalletsAsync();
        #endregion
        #region Portfolio
        Task<PortfolioResponse> GetPortfolioAsync();
        #endregion
    }
}
