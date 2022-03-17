using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Liquidity.TradingPortfolio.Domain.Interfaces;
using Service.Liquidity.TradingPortfolio.Domain.Models;

namespace Service.Liquidity.TradingPortfolio.Tests.Mocks;

public class PortfolioWalletManagerMock : IPortfolioWalletManager
{
    public PortfolioWallet GetExternalByName(string walletName)
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

    public PortfolioWallet GetInternalById(string walletId)
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

    public PortfolioWallet GetById(string walletId)
    {
        throw new NotImplementedException();
    }

    public PortfolioWallet GetByName(string walletName)
    {
        throw new NotImplementedException();
    }

    public PortfolioWallet GetByExternalSource(string externalSource)
    {
        throw new NotImplementedException();
    }

    public Task DeleteInternalByNameAsync(string walletName)
    {
        throw new NotImplementedException();
    }

    public Task DeleteExternalByNameAsync(string walletName)
    {
        throw new NotImplementedException();
    }

    public List<PortfolioWallet> Get()
    {
        throw new NotImplementedException();
    }

    Task IPortfolioWalletManager.AddExternalAsync(string walletName, string brokerId, string sourceName)
    {
        throw new NotImplementedException();
    }

    Task IPortfolioWalletManager.AddInternalAsync(string walletId, string brokerId, string walletName)
    {
        throw new NotImplementedException();
    }
}