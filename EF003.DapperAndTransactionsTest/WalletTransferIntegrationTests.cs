using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Xunit;
using EF003.DapperAndTransactions.Services;
using Dapper;
using EF003.DapperAndTransactions.Models;

namespace EF003.DapperAndTransactionsTest;

[Collection("Database")]
public class WalletTransferIntegrationTests : IDisposable
{
    private readonly IDbConnection _connection;
    private readonly IDatabaseWrapper _dbWrapper;
    private readonly IWalletTransferService _service;

    public WalletTransferIntegrationTests()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");

        // Create initial connection to master database
        var masterConnection = new SqlConnection(
            new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" }.ConnectionString);

        // Create the test database
        CreateTestDatabase(masterConnection, "DigitalCurrency_Test");

        // Now connect to the test database
        _connection = new SqlConnection(connectionString);
        _connection.Open();
        _dbWrapper = new DatabaseWrapper(_connection);
        _service = new WalletTransferService(_dbWrapper);
        
        SetupTestDatabase();
    }

    private void CreateTestDatabase(SqlConnection masterConnection, string databaseName)
    {
        var createDbQuery = $@"
            IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}')
            BEGIN
                CREATE DATABASE {databaseName};
            END";

        masterConnection.Execute(createDbQuery);
    }

    private void SetupTestDatabase()
    {
        _connection.Execute(@"
            IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Wallets')
                DROP TABLE Wallets;
            
            CREATE TABLE Wallets (
                Id INT PRIMARY KEY,
                Holder NVARCHAR(100),
                Balance DECIMAL(18,2)
            );");

        _connection.Execute(@"
            INSERT INTO Wallets (Id, Holder, Balance) VALUES 
            (1, 'Test User 1', 1000),
            (2, 'Test User 2', 500);");
    }

    [Fact]
    public void TransferMoney_ShouldUpdateBalances_WhenTransferIsValid()
    {
        // Arrange
        decimal transferAmount = 300;

        // Act
        _service.TransferMoney(1, 2, transferAmount);

        // Assert
        var wallets = _connection.Query<Wallet>("SELECT * FROM Wallets").ToDictionary(w => w.Id);
        
        Assert.Equal(700, wallets[1].Balance);
        Assert.Equal(800, wallets[2].Balance);
    }

    [Fact]
    public void TransferMoney_ShouldFail_WhenInsufficientBalance()
    {
        // Arrange
        decimal transferAmount = 1500; // More than wallet 1's balance

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _service.TransferMoney(1, 2, transferAmount));
        
        Assert.Contains("Insufficient", exception.Message);
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();

        // Optionally, clean up the test database after all tests
        /* Uncomment if you want to drop the database after tests
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection");
        var masterConnection = new SqlConnection(
            new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" }.ConnectionString);
        
        masterConnection.Execute($"DROP DATABASE IF EXISTS DigitalCurrency_Test");
        */
    }
}
