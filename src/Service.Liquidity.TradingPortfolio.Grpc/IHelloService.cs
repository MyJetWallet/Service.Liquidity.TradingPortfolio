using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Grpc.Models;

namespace Service.Liquidity.TradingPortfolio.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}