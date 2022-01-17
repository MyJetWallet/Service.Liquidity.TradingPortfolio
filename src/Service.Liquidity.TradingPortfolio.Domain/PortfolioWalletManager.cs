using MyNoSqlServer.Abstractions;
using Service.Liquidity.TradingPortfolio.Domain.Models;
using Service.Liquidity.TradingPortfolio.Domain.Models.NoSql;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public class PortfolioWalletManager : IPortfolioWalletManager
    {
        private readonly IMyNoSqlServerDataWriter<PortfolioWalletNoSql> _myNoSqlWalletWriter;
        private Dictionary<string, PortfolioWallet> _wallets = new Dictionary<string,PortfolioWallet>();

        
        public PortfolioWalletManager(IMyNoSqlServerDataWriter<PortfolioWalletNoSql> myNoSqlWalletWriter)
        {
            _myNoSqlWalletWriter = myNoSqlWalletWriter;
        }

        public void Load()
        {
            var data = _myNoSqlWalletWriter.GetAsync().GetAwaiter().GetResult();
            _wallets = data.Select(e => e.Wallet).ToDictionary(e => e.Name);
        }

        public async Task AddExternalWallet(string walletName, string brokerId, string sourceName)
        {
            var portfolioWallet = new PortfolioWallet()
            {
                IsInternal = false,
                ExternalSource = sourceName,
                Name = walletName,
                WalletId = walletName
            };
            _wallets[portfolioWallet.Name] = portfolioWallet;
            await _myNoSqlWalletWriter.InsertOrReplaceAsync(PortfolioWalletNoSql.Create(portfolioWallet));
        }

        public async Task AddInternalWallet(string walletId, string brokerId, string walletName)
        {
            var portfolioWallet = new PortfolioWallet()
            {
                IsInternal = true,
                ExternalSource = null,
                Name = walletName,
                WalletId = walletId,
                BrokerId = brokerId
            };
            _wallets[portfolioWallet.Name] = portfolioWallet;
            await _myNoSqlWalletWriter.InsertOrReplaceAsync(PortfolioWalletNoSql.Create(portfolioWallet));
        }

        public PortfolioWallet GetExternalWalletByWalletName(string walletName)
        {
            if (!_wallets.TryGetValue(walletName, out var wallet))
            {
                return null;
            }
            return !wallet.IsInternal ? wallet : null;
        }

        public PortfolioWallet GetExternalWalletByWalletId(string walletId)
        {
            var wallet = GetWalletByWalletId(walletId);
            if (wallet == null)
            {
                return null;
            }
            return !wallet.IsInternal ? wallet : null;

        }

        public PortfolioWallet GetInternalWalletByWalletName(string walletName)
        {
            if (!_wallets.TryGetValue(walletName, out var wallet))
            {
                return null;
            }

            return wallet.IsInternal ? wallet : null;
        }

        public PortfolioWallet GetInternalWalletByWalletId(string walletId)
        {
            var wallet = GetWalletByWalletId(walletId);
            if (wallet == null)
            {
                return null;
            }
            return wallet.IsInternal ? wallet : null;
        }

        public PortfolioWallet GetWalletByWalletId(string walletId)
        {
            foreach (var wallet in _wallets.Values)
            {
                if (wallet.WalletId == walletId)
                    return wallet;
            }
            return null;
        }

        public PortfolioWallet GetWalletByWalletName(string walletName)
        {
            if (!_wallets.TryGetValue(walletName, out var wallet))
            {
                return null;
            }

            return wallet;
        }

        public async Task DeleteInternalWalletByWalletName(string walletName)
        {
            if (!_wallets.TryGetValue(walletName, out var wallet))
            {
                return;
            }

            if (wallet.IsInternal)
            {
                await _myNoSqlWalletWriter.DeleteAsync(PortfolioWalletNoSql.GeneratePartitionKey(), 
                    PortfolioWalletNoSql.GenerateRowKey(walletName));
                
                var isRemoved = _wallets.Remove(walletName);
            }
        }

        public async Task DeleteExternalWalletByWalletName(string walletName)
        {
            if (!_wallets.TryGetValue(walletName, out var wallet))
            {
                return;
            }

            if (wallet.IsInternal == false)
            {
                await _myNoSqlWalletWriter.DeleteAsync(PortfolioWalletNoSql.GeneratePartitionKey(),
                    PortfolioWalletNoSql.GenerateRowKey(walletName));

                var isRemoved = _wallets.Remove(walletName);
            }
        }

        public List<PortfolioWallet> GetWallets()
        {
            return _wallets.Values.ToList();
        }
    }
}
