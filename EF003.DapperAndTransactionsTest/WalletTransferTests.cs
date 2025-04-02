using Moq;
using Xunit;
using EF003.DapperAndTransactions.Services;
using EF003.DapperAndTransactions.Models;

namespace EF003.DapperAndTransactionsTest;

public class WalletTransferTests
{
    private readonly Mock<IDatabaseWrapper> _mockDb;
    private readonly IWalletTransferService _service;

    public WalletTransferTests()
    {
        _mockDb = new Mock<IDatabaseWrapper>();
        _service = new WalletTransferService(_mockDb.Object);
    }

    [Theory]
    [InlineData(1, 2, 500, 1000, 500, true)]      // Valid transfer
    [InlineData(1, 2, 1500, 1000, 500, false)]    // Insufficient balance
    [InlineData(1, 2, -100, 1000, 500, false)]    // Negative amount
    [InlineData(1, 2, 0, 1000, 500, false)]       // Zero amount
    public void TransferMoney_ShouldHandleValidAndInvalidCases(
        int fromId, int toId, decimal amount,
        decimal fromBalance, decimal toBalance,
        bool shouldSucceed)
    {
        // Arrange
        var fromWallet = new Wallet { Id = fromId, Balance = fromBalance, Holder = "Test1" };
        var toWallet = new Wallet { Id = toId, Balance = toBalance, Holder = "Test2" };

        _mockDb.Setup(db => db.GetWallet(fromId)).Returns(fromWallet);
        _mockDb.Setup(db => db.GetWallet(toId)).Returns(toWallet);
        _mockDb.Setup(db => db.UpdateWalletBalance(It.IsAny<int>(), It.IsAny<decimal>()))
            .Returns(1);

        if (shouldSucceed)
        {
            // Act
            _service.TransferMoney(fromId, toId, amount);

            // Assert
            _mockDb.Verify(db => db.UpdateWalletBalance(fromId, fromBalance - amount), Times.Once);
            _mockDb.Verify(db => db.UpdateWalletBalance(toId, toBalance + amount), Times.Once);
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => 
                _service.TransferMoney(fromId, toId, amount));
        }
    }

    [Fact]
    public void TransferMoney_ShouldThrow_WhenWalletNotFound()
    {
        // Arrange
        _mockDb.Setup(db => db.GetWallet(It.IsAny<int>()))
            .Throws(new InvalidOperationException("Wallet not found"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => _service.TransferMoney(1, 2, 100));
        Assert.Equal("Wallet not found", exception.Message);
    }

    [Fact]
    public void TransferMoney_ShouldThrow_WhenUpdateFails()
    {
        // Arrange
        var fromWallet = new Wallet { Id = 1, Balance = 1000, Holder = "Test1" };
        var toWallet = new Wallet { Id = 2, Balance = 500, Holder = "Test2" };

        _mockDb.Setup(db => db.GetWallet(1)).Returns(fromWallet);
        _mockDb.Setup(db => db.GetWallet(2)).Returns(toWallet);
        _mockDb.Setup(db => db.UpdateWalletBalance(It.IsAny<int>(), It.IsAny<decimal>()))
            .Returns(0);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => _service.TransferMoney(1, 2, 100));
        Assert.Contains("failed", exception.Message.ToLower());
    }

    [Fact]
    public void TransferMoney_ShouldVerifyTransactionFlow()
    {
        // Arrange
        var fromWallet = new Wallet { Id = 1, Balance = 1000, Holder = "Test1" };
        var toWallet = new Wallet { Id = 2, Balance = 500, Holder = "Test2" };
        var sequence = new MockSequence();

        _mockDb.Setup(db => db.GetWallet(1)).Returns(fromWallet);
        _mockDb.Setup(db => db.GetWallet(2)).Returns(toWallet);
        
        _mockDb.InSequence(sequence)
            .Setup(db => db.UpdateWalletBalance(1, 900))
            .Returns(1);
        
        _mockDb.InSequence(sequence)
            .Setup(db => db.UpdateWalletBalance(2, 600))
            .Returns(1);

        // Act
        _service.TransferMoney(1, 2, 100);

        // Assert
        _mockDb.VerifyAll();
    }
}