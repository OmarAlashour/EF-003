using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using EF003.DapperAndTransactions.Services;
using EF003.DapperAndTransactions.Models;
using Dapper;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

string connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

using IDbConnection connection = new SqlConnection(connectionString);
var dbWrapper = new DatabaseWrapper(connection);
var walletService = new WalletTransferService(dbWrapper);

try
{
    // Ensure database and table exist
    await EnsureDatabaseSetup(connection);

    // Display menu
    while (true)
    {
        Console.Clear();
        Console.WriteLine("Wallet Transfer System");
        Console.WriteLine("1. View all wallets");
        Console.WriteLine("2. Transfer money");
        Console.WriteLine("3. Exit");
        Console.Write("Select an option: ");

        string? choice = Console.ReadLine();
        
        switch (choice)
        {
            case "1":
                await DisplayWallets(connection);
                break;
            case "2":
                await PerformTransfer(walletService, connection);
                break;
            case "3":
                return;
            default:
                Console.WriteLine("Invalid option");
                break;
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

static async Task EnsureDatabaseSetup(IDbConnection connection)
{
    var sql = @"
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wallets')
        BEGIN
            CREATE TABLE Wallets (
                Id INT PRIMARY KEY,
                Holder NVARCHAR(100),
                Balance DECIMAL(18,2)
            );
            
            INSERT INTO Wallets (Id, Holder, Balance) VALUES 
            (1, 'Omar Alashour', 10000),
            (2, 'Khalid Alashour', 3500),
            (3, 'Ali Hassan', 6500);
        END";

    await connection.ExecuteAsync(sql);
}

static async Task DisplayWallets(IDbConnection connection)
{
    var wallets = await connection.QueryAsync<Wallet>("SELECT * FROM Wallets");
    Console.WriteLine("\nCurrent Wallets:");
    foreach (var wallet in wallets)
    {
        Console.WriteLine(wallet);
    }
}

static async Task PerformTransfer(IWalletTransferService walletService, IDbConnection connection)
{
    try
    {
        Console.Write("Enter source wallet ID: ");
        if (!int.TryParse(Console.ReadLine(), out int fromId))
            throw new ArgumentException("Invalid source wallet ID");

        Console.Write("Enter target wallet ID: ");
        if (!int.TryParse(Console.ReadLine(), out int toId))
            throw new ArgumentException("Invalid target wallet ID");

        Console.Write("Enter amount to transfer: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            throw new ArgumentException("Invalid amount");

        walletService.TransferMoney(fromId, toId, amount);
        Console.WriteLine("Transfer completed successfully!");
        
        await DisplayWallets(connection);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Transfer failed: {ex.Message}");
    }
}