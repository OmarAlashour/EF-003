using System.Data;
using System.Transactions;
using EF003.DapperAndTransactions.Models;

namespace EF003.DapperAndTransactions.Services;

public interface IWalletTransferService
{
    void TransferMoney(int fromId, int toId, decimal amount);
}

public class WalletTransferService : IWalletTransferService
{
    private readonly IDatabaseWrapper _db;

    public WalletTransferService(IDatabaseWrapper db)
    {
        _db = db;
    }

    public void TransferMoney(int fromId, int toId, decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Transfer amount must be positive");

        using var scope = new TransactionScope();
        
        var fromWallet = _db.GetWallet(fromId);
        var toWallet = _db.GetWallet(toId);

        if (fromWallet.Balance < amount)
            throw new InvalidOperationException("Insufficient balance for transfer");

        int updatedRows = _db.UpdateWalletBalance(fromId, fromWallet.Balance - amount);
        if (updatedRows == 0)
            throw new InvalidOperationException("Update failed for source wallet");

        updatedRows = _db.UpdateWalletBalance(toId, toWallet.Balance + amount);
        if (updatedRows == 0)
            throw new InvalidOperationException("Update failed for target wallet");

        scope.Complete();
    }
}
