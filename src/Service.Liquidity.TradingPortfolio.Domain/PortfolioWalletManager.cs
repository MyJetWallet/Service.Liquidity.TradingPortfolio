﻿using Service.Liquidity.TradingPortfolio.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain.Interfaces;

namespace Service.Liquidity.TradingPortfolio.Domain
{
    public class PortfolioWalletManager : IPortfolioWalletManager
    {
        private readonly IPortfolioWalletsStorage _portfolioWalletsStorage;
        private Dictionary<string, PortfolioWallet> _wallets = new ();

        public PortfolioWalletManager(
            IPortfolioWalletsStorage portfolioWalletsStorage
            )
        {
            _portfolioWalletsStorage = portfolioWalletsStorage;
        }

        public void Load()
        {
            var data = _portfolioWalletsStorage.GetAsync().GetAwaiter().GetResult();
            _wallets = data.Select(e => e).ToDictionary(e => e.Name);
        }

        public async Task AddExternalWallet(string walletName, string brokerId, string sourceName)
        {
            var portfolioWallet = new PortfolioWallet
            {
                IsInternal = false,
                ExternalSource = sourceName,
                Name = walletName,
                WalletId = walletName
            };
            _wallets[portfolioWallet.Name] = portfolioWallet;
            await _portfolioWalletsStorage.AddOrUpdateAsync(portfolioWallet);
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
            await _portfolioWalletsStorage.AddOrUpdateAsync(portfolioWallet);
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
                await _portfolioWalletsStorage.DeleteAsync(walletName);
                _wallets.Remove(walletName);
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
                await _portfolioWalletsStorage.DeleteAsync(walletName);
                _wallets.Remove(walletName);
            }
        }

        public List<PortfolioWallet> GetWallets()
        {
            return _wallets.Values.ToList();
        }
        
        public PortfolioWallet GetWalletByExternalSource(string externalSource)
        {
            return _wallets.Values.FirstOrDefault(w => w.ExternalSource == externalSource);
        }
    }
}
