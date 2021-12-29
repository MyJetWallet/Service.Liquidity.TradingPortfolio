using Service.Liquidity.TradingPortfolio.Grpc.Models;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Grpc
{
    [ServiceContract]
    public interface IManualInputService
    {
        [OperationContract]
        Task<DailyVelocityResponse> SetDailyVelocityAsync(DailyVelocityRequest request);
    }
}
