using Lab4.Core;

namespace Lab4.Tests;

/// <summary>Общее поведение Account (проверяем через DebitAccount) и DebitAccount.</summary>
public class AccountTests
{
    // Вспомогательный метод: создаёт дебетовый счёт, чтобы не повторять это в каждом тесте
    private static DebitAccount CreateDebit(decimal balance = 1000m, decimal cashback = 0.01m)
    {
        return new DebitAccount("D-001", "Иванов Иван", balance, cashback);
    }

    //  Конструктор / инварианты 

    [Fact]
    public void Constructor_ValidArguments_SetsProperties()
    {
        // Act
        DebitAccount account = new DebitAccount("D-001", "Иванов Иван", 500m, 0.01m);

        // Assert
        Assert.Equal("D-001", account.AccountNumber);
        Assert.Equal("Иванов Иван", account.OwnerName);
        Assert.Equal(500m, account.Balance);
        Assert.Equal(0.01m, account.CashbackRate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyAccountNumber_Throws(string? number)
    {
        Assert.Throws<ArgumentException>(() => new DebitAccount(number!, "Иванов", 100m, 0.01m));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_EmptyOwnerName_Throws(string? owner)
    {
        Assert.Throws<ArgumentException>(() => new DebitAccount("D-1", owner!, 100m, 0.01m));
    }

    [Fact]
    public void Constructor_NegativeInitialBalance_Throws()
    {
        Assert.Throws<ArgumentException>(() => new DebitAccount("D-1", "Иванов", -0.01m, 0.01m));
    }

    [Fact]
    public void Constructor_ZeroInitialBalance_IsAllowed()
    {
        // Граничный случай: ноль допустим
        DebitAccount account = new DebitAccount("D-1", "Иванов", 0m, 0.01m);

        Assert.Equal(0m, account.Balance);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.0)]   // верхняя граница не включается
    [InlineData(2.5)]
    public void Constructor_InvalidCashbackRate_Throws(double rate)
    {
        Assert.Throws<ArgumentException>(() => new DebitAccount("D-1", "Иванов", 100m, (decimal)rate));
    }

    [Fact]
    public void Constructor_ZeroCashbackRate_IsAllowed()
    {
        DebitAccount account = new DebitAccount("D-1", "Иванов", 100m, 0m);

        Assert.Equal(0m, account.CashbackRate);
    }

    //  Deposit 

    [Fact]
    public void Deposit_PositiveAmount_IncreasesBalance()
    {
        // Arrange
        DebitAccount account = CreateDebit(1000m);

        // Act
        account.Deposit(250m);

        // Assert
        Assert.Equal(1250m, account.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Deposit_NonPositiveAmount_ThrowsAndKeepsBalance(double amount)
    {
        // Arrange
        DebitAccount account = CreateDebit(1000m);

        // Act + Assert
        Assert.Throws<ArgumentException>(() => account.Deposit((decimal)amount));
        Assert.Equal(1000m, account.Balance);
    }

    //  Withdraw (дебетовый) 

    [Fact]
    public void Withdraw_LessThanBalance_DecreasesBalanceAndReturnsAmount()
    {
        // Arrange
        DebitAccount account = CreateDebit(1000m);

        // Act
        decimal withdrawn = account.Withdraw(300m);

        // Assert
        Assert.Equal(300m, withdrawn);
        Assert.Equal(700m, account.Balance);
    }

    [Fact]
    public void Withdraw_ExactlyWholeBalance_LeavesZero()
    {
        // Граничный случай: снимаем ровно весь остаток
        DebitAccount account = CreateDebit(1000m);

        account.Withdraw(1000m);

        Assert.Equal(0m, account.Balance);
    }

    [Fact]
    public void Withdraw_MoreThanBalance_ThrowsAndKeepsBalance()
    {
        // Arrange
        DebitAccount account = CreateDebit(1000m);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => account.Withdraw(1000.01m));
        Assert.Equal(1000m, account.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Withdraw_NonPositiveAmount_Throws(double amount)
    {
        DebitAccount account = CreateDebit(1000m);

        Assert.Throws<ArgumentException>(() => account.Withdraw((decimal)amount));
    }

    //  ApplyInterest / сравнение 

    [Fact]
    public void ApplyInterest_Debit_AddsCashback()
    {
        // Arrange: 1000 * 1% = 10
        DebitAccount account = CreateDebit(1000m, 0.01m);

        // Act
        account.ApplyInterest();

        // Assert
        Assert.Equal(1010m, account.Balance);
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        DebitAccount account = CreateDebit();

        Assert.True(account.CompareTo(null) > 0);
    }

    [Fact]
    public void CompareTo_SmallerBalance_ReturnsNegative()
    {
        DebitAccount poor = CreateDebit(100m);
        DebitAccount rich = new DebitAccount("D-2", "Петров", 900m, 0.01m);

        Assert.True(poor.CompareTo(rich) < 0);
        Assert.True(rich.CompareTo(poor) > 0);
    }

    [Fact]
    public void ToString_ContainsTypeNumberAndOwner()
    {
        // Не проверяем формат валюты — он зависит от культуры компьютера
        string text = CreateDebit().ToString();

        Assert.Contains("DebitAccount", text);
        Assert.Contains("D-001", text);
        Assert.Contains("Иванов Иван", text);
    }
}
