using Lab4.Core;

namespace Lab4.Tests;

public class CreditAccountTests
{
    private static CreditAccount CreateCredit(decimal balance = 0m, decimal limit = 1000m, decimal rate = 0.02m)
    {
        return new CreditAccount("C-001", "Сидоров Алексей", balance, limit, rate);
    }

    [Fact]
    public void Withdraw_WithinLimit_BalanceBecomesNegative()
    {
        // Arrange
        CreditAccount account = CreateCredit(balance: 5000m, limit: 100000m);

        // Act
        decimal withdrawn = account.Withdraw(30000m);

        // Assert
        Assert.Equal(30000m, withdrawn);
        Assert.Equal(-25000m, account.Balance);
    }

    [Fact]
    public void Withdraw_ExactlyToCreditLimit_IsAllowed()
    {
        // Граничный случай: уходим ровно на -лимит
        CreditAccount account = CreateCredit(balance: 0m, limit: 1000m);

        account.Withdraw(1000m);

        Assert.Equal(-1000m, account.Balance);
    }

    [Fact]
    public void Withdraw_BeyondCreditLimit_ThrowsAndKeepsBalance()
    {
        // Arrange
        CreditAccount account = CreateCredit(balance: 0m, limit: 1000m);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => account.Withdraw(1000.01m));
        Assert.Equal(0m, account.Balance);
    }

    [Fact]
    public void Withdraw_NonPositiveAmount_Throws()
    {
        CreditAccount account = CreateCredit();

        Assert.Throws<ArgumentException>(() => account.Withdraw(0m));
    }

    [Fact]
    public void ApplyInterest_OnDebt_IncreasesDebt()
    {
        // Arrange: долг 1000, ставка 2% → долг 1020
        CreditAccount account = CreateCredit(balance: 0m, limit: 5000m, rate: 0.02m);
        account.Withdraw(1000m);

        // Act
        account.ApplyInterest();

        // Assert
        Assert.Equal(-1020m, account.Balance);
    }

    [Fact]
    public void ApplyInterest_PositiveBalance_DoesNotChangeBalance()
    {
        CreditAccount account = CreateCredit(balance: 500m);

        account.ApplyInterest();

        Assert.Equal(500m, account.Balance);
    }

    [Theory]
    [InlineData(0, 0.02)]    // лимит должен быть > 0
    [InlineData(-100, 0.02)]
    [InlineData(1000, 0)]    // ставка должна быть > 0
    [InlineData(1000, 1)]    // ... и < 1
    [InlineData(1000, -0.5)]
    public void Constructor_InvalidLimitOrRate_Throws(double limit, double rate)
    {
        Assert.Throws<ArgumentException>(
            () => new CreditAccount("C-1", "Сидоров", 0m, (decimal)limit, (decimal)rate));
    }
}
