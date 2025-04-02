using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Dapper;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

IDbConnection db = new SqlConnection(configuration.GetSection("constr").Value);

var walletToDelete = new Wallet
{
    Id = 14
};

var sql = "DELETE FROM Wallets WHERE Id = @Id;";

var parameters = 
    new
    {
        Id = walletToDelete.Id
    };

try
{
    db.Execute(sql, parameters);
    Console.WriteLine("Delete operation completed successfully.");
}
catch (SqlException ex)
{
    Console.WriteLine($"Database error occurred: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

Console.ReadKey();