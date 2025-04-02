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
    Id = 9
};

var sql = "DELETE FROM Wallets WHERE Id = @Id;";

var parameters = 
    new
    {
        Id = walletToDelete.Id
    };

db.Execute(sql, parameters);

Console.ReadKey();