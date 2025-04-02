using System.Data;
using Dapper;
using EF003.DapperAndTransactions.Models;

namespace EF003.DapperAndTransactions.Services;

public class DatabaseWrapper : IDatabaseWrapper
{
    private readonly IDbConnection _db;

    public DatabaseWrapper(IDbConnection db)
    {
        _db = db;
    }

    public Wallet GetWallet(int id)
    {
        return _db.QuerySingle<Wallet>("SELECT * FROM Wallets WHERE Id = @Id", new { Id = id });
    }

    public int UpdateWalletBalance(int id, decimal newBalance)
    {
        return _db.Execute(
            "UPDATE Wallets SET Balance = @Balance WHERE Id = @Id",
            new { Id = id, Balance = newBalance });
    }
}
