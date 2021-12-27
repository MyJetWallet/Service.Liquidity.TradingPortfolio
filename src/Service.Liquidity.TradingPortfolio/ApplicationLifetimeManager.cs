using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;

namespace Service.Liquidity.TradingPortfolio
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly IHostApplicationLifetime appLifetime;
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly ServiceBusLifeTime _serviceBusLifeTime;
        private readonly MyNoSqlClientLifeTime _myNoSqlClientLifeTime;

        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime, 
            ILogger<ApplicationLifetimeManager> logger,
            ServiceBusLifeTime serviceBusLifeTime,
            MyNoSqlClientLifeTime myNoSqlClientLifeTime)
            : base(appLifetime)
        {
            this.appLifetime = appLifetime;
            _logger = logger;
            _serviceBusLifeTime = serviceBusLifeTime;
            _myNoSqlClientLifeTime = myNoSqlClientLifeTime;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            _myNoSqlClientLifeTime.Start();
            _serviceBusLifeTime.Start();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");
            _serviceBusLifeTime.Stop();
            _myNoSqlClientLifeTime.Stop();
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
