using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain.Interfaces;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Domain.Services
{
    public class PortfolioWalletManager : IPortfolioWalletManager
    {
        private readonly IPortfolioWalletsStorage _portfolioWalletsStorage;
        private Dictionary<string, PortfolioWallet> _walletsByName = new ();

        public PortfolioWalletManager(
            IPortfolioWalletsStorage portfolioWalletsStorage
            )
        {
            _portfolioWalletsStorage = portfolioWalletsStorage;
        }

        public void Load()
        {
            var data = _portfolioWalletsStorage.GetAsync().GetAwaiter().GetResult();
            _walletsByName = data.Select(e => e).ToDictionary(e => e.Name);
        }

        public async Task AddExternalAsync(string walletName, string brokerId, string sourceName)
        {
            var portfolioWallet = new PortfolioWallet
            {
                IsInternal = false,
                ExternalSource = sourceName,
                Name = walletName,
                WalletId = walletName
            };
            _walletsByName[portfolioWallet.Name] = portfolioWallet;
            await _portfolioWalletsStorage.AddOrUpdateAsync(portfolioWallet);
        }

        public async Task AddInternalAsync(string walletId, string brokerId, string walletName)
        {
            var portfolioWallet = new PortfolioWallet
            {
                IsInternal = true,
                ExternalSource = null,
                Name = walletName,
                WalletId = walletId,
                BrokerId = brokerId
            };
            _walletsByName[portfolioWallet.Name] = portfolioWallet;
            await _portfolioWalletsStorage.AddOrUpdateAsync(portfolioWallet);
        }

        public PortfolioWallet GetExternalByName(string walletName)
        {
            if (!_walletsByName.TryGetValue(walletName, out var wallet))
            {
                return null;
            }
            return !wallet.IsInternal ? wallet : null;
        }

        public PortfolioWallet GetExternalWalletByWalletId(string walletId)
        {
            var wallet = GetById(walletId);
            if (wallet == null)
            {
                return null;
            }
            return !wallet.IsInternal ? wallet : null;

        }

        public PortfolioWallet GetInternalWalletByWalletName(string walletName)
        {
            if (!_walletsByName.TryGetValue(walletName, out var wallet))
            {
                return null;
            }

            return wallet.IsInternal ? wallet : null;
        }

        public PortfolioWallet GetInternalById(string walletId)
        {
            var wallet = GetById(walletId);
            if (wallet == null)
            {
                return null;
            }
            return wallet.IsInternal ? wallet : null;
        }

        public PortfolioWallet GetById(string walletId)
        {
            foreach (var wallet in _walletsByName.Values)
            {
                if (wallet.WalletId == walletId)
                    return wallet;
            }
            return null;
        }

        public PortfolioWallet GetByName(string walletName)
        {
            if (!_walletsByName.TryGetValue(walletName, out var wallet))
            {
                return null;
            }

            return wallet;
        }

        public async Task DeleteInternalByNameAsync(string walletName)
        {
            if (!_walletsByName.TryGetValue(walletName, out var wallet))
            {
                return;
            }

            if (wallet.IsInternal)
            {
                await _portfolioWalletsStorage.DeleteAsync(walletName);
                _walletsByName.Remove(walletName);
            }
        }

        public async Task DeleteExternalByNameAsync(string walletName)
        {
            if (!_walletsByName.TryGetValue(walletName, out var wallet))
            {
                return;
            }

            if (wallet.IsInternal == false)
            {
                await _portfolioWalletsStorage.DeleteAsync(walletName);
                _walletsByName.Remove(walletName);
            }
        }

        public List<PortfolioWallet> Get()
        {
            return _walletsByName.Values.ToList();
        }
        
        public PortfolioWallet GetByExternalSource(string externalSource)
        {
            return _walletsByName.Values.FirstOrDefault(w => w.ExternalSource == externalSource);
        }
    }
}
