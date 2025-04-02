using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Dapper;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

IDbConnection db = new SqlConnection(configuration.GetSection("constr").Value);

// Read wallet details from the console
int id;
do
{
    Console.Write("Enter Wallet ID (number): ");
    if (!int.TryParse(Console.ReadLine(), out id))
    {
        Console.WriteLine("Invalid ID format. Please enter a valid number.");
    }
    else
    {
        break;
    }
} while (true);

string? holder;
do
{
    Console.Write("Enter Wallet Holder Name: ");
    holder = Console.ReadLine() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(holder))
    {
        Console.WriteLine("Wallet Holder Name cannot be empty. Please try again.");
    }
} while (string.IsNullOrWhiteSpace(holder));

Console.Write("Enter Wallet Balance: ");
if (!decimal.TryParse(Console.ReadLine(), out decimal balance))
{
    Console.WriteLine("Invalid balance. Please enter a valid decimal number.");
    return;
}

var walletToInsert = new Wallet { Id = id, Holder = holder, Balance = balance };

var sql = "INSERT INTO Wallets (Id, Holder, Balance) " +
          "VALUES (@Id, @Holder, @Balance)";

try
{
    db.Execute(sql,
        new
        {
            Id = walletToInsert.Id,
            Holder = walletToInsert.Holder,
            Balance = walletToInsert.Balance
        });

    Console.WriteLine("Wallet inserted successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while inserting the wallet: {ex.Message}");
}

Console.ReadKey();