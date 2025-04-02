using EF003.DapperAndTransactions.Models;

namespace EF003.DapperAndTransactions.Services;

public interface IDatabaseWrapper
{
    Wallet GetWallet(int id);
    int UpdateWalletBalance(int id, decimal newBalance);
}
