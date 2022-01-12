using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using Service.Liquidity.TradingPortfolio.Domain;

namespace Service.Liquidity.TradingPortfolio
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly IHostApplicationLifetime appLifetime;
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly ServiceBusLifeTime _serviceBusLifeTime;
        private readonly MyNoSqlClientLifeTime _myNoSqlClientLifeTime;
        private readonly PortfolioWalletManager _portfolioWalletManager;
        private readonly PortfolioManager _portfolioManager;

        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime, 
            ILogger<ApplicationLifetimeManager> logger,
            ServiceBusLifeTime serviceBusLifeTime,
            MyNoSqlClientLifeTime myNoSqlClientLifeTime,
            PortfolioWalletManager portfolioWalletManager, 
            PortfolioManager portfolioManager)
            : base(appLifetime)
        {
            this.appLifetime = appLifetime;
            _logger = logger;
            _serviceBusLifeTime = serviceBusLifeTime;
            _myNoSqlClientLifeTime = myNoSqlClientLifeTime;
            _portfolioWalletManager = portfolioWalletManager;
            _portfolioManager = portfolioManager;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            _myNoSqlClientLifeTime.Start();
            _serviceBusLifeTime.Start();
            _portfolioManager.Load();
            _portfolioWalletManager.Load();
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
