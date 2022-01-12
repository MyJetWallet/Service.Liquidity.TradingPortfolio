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
            _wallets = data.Select(e => e.Wallet).ToDictionary(e => e.Id);
        }

        public async Task AddExternalWallet(string walletName, string brokerId, string sourceName)
        {
            var portfolioWallet = new PortfolioWallet()
            {
                IsInternal = false,
                ExternalSource = sourceName,
                Id = walletName,
            };
            _wallets[portfolioWallet.Id] = portfolioWallet;
            await _myNoSqlWalletWriter.InsertOrReplaceAsync(PortfolioWalletNoSql.Create(portfolioWallet));
        }

        public async Task AddInternalWallet(string walletId, string brokerId, string walletName)
        {
            var portfolioWallet = new PortfolioWallet()
            {
                IsInternal = true,
                ExternalSource = null,
                Id = walletName,
                InternalWalletId = walletId,
                BrokerId = brokerId
            };
            _wallets[portfolioWallet.Id] = portfolioWallet;
            await _myNoSqlWalletWriter.InsertOrReplaceAsync(PortfolioWalletNoSql.Create(portfolioWallet));
        }

        public PortfolioWallet GetExternalWalletByWalletId(string walletId)
        {
            if (!_wallets.TryGetValue(walletId, out var wallet))
            {
                return null;
            }

            return wallet.IsInternal == false ? wallet : null;
        }

        public PortfolioWallet GetInternalWalletByWalletId(string walletId)
        {
            if(!_wallets.TryGetValue(walletId, out var wallet))
            {
                return null;    
            }
            
            return wallet.IsInternal ==  true ? wallet : null;
        }

        public PortfolioWallet GetWalleteByWalletId(string walletId)
        {
            if (!_wallets.TryGetValue(walletId, out var wallet))
            {
                return null;
            }

            return wallet;
        }

        public async Task DeleteInternalWallet(string walletId)
        {
            if (!_wallets.TryGetValue(walletId, out var wallet))
            {
                return;
            }

            if (wallet.IsInternal)
            {
                await _myNoSqlWalletWriter.DeleteAsync(PortfolioWalletNoSql.GeneratePartitionKey(), 
                    PortfolioWalletNoSql.GenerateRowKey(walletId));
                
                var isRemoved = _wallets.Remove(walletId);
            }
        }

        public async Task DeleteExternalWallet(string walletId)
        {
            if (!_wallets.TryGetValue(walletId, out var wallet))
            {
                return;
            }

            if (wallet.IsInternal == false)
            {
                await _myNoSqlWalletWriter.DeleteAsync(PortfolioWalletNoSql.GeneratePartitionKey(),
                    PortfolioWalletNoSql.GenerateRowKey(walletId));

                var isRemoved = _wallets.Remove(walletId);
            }
        }

        public List<PortfolioWallet> GetWallets()
        {
            return _wallets.Values.ToList();
        }
    }
}
