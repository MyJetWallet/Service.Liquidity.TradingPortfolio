using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests;

public class PortfolioWalletManagerMock : IPortfolioWalletManager
{
    public PortfolioWallet GetExternalWalletByWalletName(string walletName)
    {
        if (walletName == "Converter")
            return new Domain.Models.PortfolioWallet()
            {
                IsInternal = true,
                ExternalSource = null,
                Name = "Converter",
                WalletId = "SP-Broker"
            };

        return null;
    }

    public PortfolioWallet GetExternalWalletByWalletId(string walletId)
    {
        if (walletId == "SP-Broker")
            return new Domain.Models.PortfolioWallet()
            {
                IsInternal = true,
                ExternalSource = null,
                Name = "Converter",
                WalletId = "SP-Broker"
            };

        return null;
    }

    public PortfolioWallet GetInternalWalletByWalletName(string walletName)
    {
        if (walletName == "Converter")
            return new Domain.Models.PortfolioWallet()
            {
                IsInternal = true,
                ExternalSource = null,
                Name = "Converter",
                WalletId = "SP-Broker"
            };

        if (walletName == "Converter-1")
            return new Domain.Models.PortfolioWallet()
            {
                IsInternal = true,
                ExternalSource = null,
                Name = "Converter-1",
                WalletId = "SP-Broker-1"
            };
        return null;
    }

    public PortfolioWallet GetInternalWalletByWalletId(string walletId)
    {
        if (walletId == "SP-Broker")
            return new Domain.Models.PortfolioWallet()
            {
                IsInternal = true,
                ExternalSource = null,
                Name = "Converter",
                WalletId = "SP-Broker"
            };

        if (walletId == "SP-Broker-1")
            return new Domain.Models.PortfolioWallet()
            {
                IsInternal = true,
                ExternalSource = null,
                Name = "Converter-1",
                WalletId = "SP-Broker-1"
            };
        return null;
    }

    public PortfolioWallet GetWalletByWalletId(string walletId)
    {
        throw new NotImplementedException();
    }

    public PortfolioWallet GetWalletByWalletName(string walletName)
    {
        throw new NotImplementedException();
    }

    public PortfolioWallet GetWalletByExternalSource(string externalSource)
    {
        throw new NotImplementedException();
    }

    public Task DeleteInternalWalletByWalletName(string walletName)
    {
        throw new NotImplementedException();
    }

    public Task DeleteExternalWalletByWalletName(string walletName)
    {
        throw new NotImplementedException();
    }

    public List<PortfolioWallet> GetWallets()
    {
        throw new NotImplementedException();
    }

    Task IPortfolioWalletManager.AddExternalWallet(string walletName, string brokerId, string sourceName)
    {
        throw new NotImplementedException();
    }

    Task IPortfolioWalletManager.AddInternalWallet(string walletId, string brokerId, string walletName)
    {
        throw new NotImplementedException();
    }
}