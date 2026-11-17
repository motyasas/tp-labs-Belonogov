using Lab4.Core;

namespace Lab4.Tests;

public class BankTests
{
    private static DebitAccount Debit(string number, string owner, decimal balance, decimal cashback = 0.01m)
    {
        return new DebitAccount(number, owner, balance, cashback);
    }

    private static CreditAccount Credit(string number, string owner, decimal balance = 0m)
    {
        return new CreditAccount(number, owner, balance, 10000m, 0.02m);
    }

    // ───────────── Конструктор ─────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyName_Throws(string? name)
    {
        Assert.Throws<ArgumentException>(() => new Bank(name!));
    }

    // ───────────── OpenAccount / FindByNumber ─────────────

    [Fact]
    public void OpenAccount_NewAccount_CanBeFoundByNumber()
    {
        // Arrange
        Bank bank = new Bank("Альфа");
        DebitAccount account = Debit("D-001", "Иванов", 100m);

        // Act
        bank.OpenAccount(account);

        // Assert
        Assert.Same(account, bank.FindByNumber("D-001"));
    }

    [Fact]
    public void OpenAccount_DuplicateNumber_Throws()
    {
        // Arrange
        Bank bank = new Bank("Альфа");
        bank.OpenAccount(Debit("D-001", "Иванов", 100m));

        // Act + Assert
        Assert.Throws<ArgumentException>(() => bank.OpenAccount(Credit("D-001", "Петров")));
    }

    [Fact]
    public void OpenAccount_Null_Throws()
    {
        Bank bank = new Bank("Альфа");

        Assert.Throws<ArgumentException>(() => bank.OpenAccount(null!));
    }

    [Fact]
    public void FindByNumber_UnknownNumber_ReturnsNull()
    {
        Bank bank = new Bank("Альфа");
        bank.OpenAccount(Debit("D-001", "Иванов", 100m));

        Assert.Null(bank.FindByNumber("X-999"));
    }

    // ───────────── GetByOwner ─────────────

    [Fact]
    public void GetByOwner_PartOfName_ReturnsAllMatching()
    {
        // Arrange
        Bank bank = new Bank("Альфа");
        bank.OpenAccount(Debit("D-001", "Иванов Иван", 100m));
        bank.OpenAccount(Debit("D-002", "Петрова Мария", 200m));
        bank.OpenAccount(Credit("C-001", "Иванов Иван"));

        // Act
        List<Account> result = bank.GetByOwner("Иванов");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Contains("Иванов", a.OwnerName));
    }

    [Fact]
    public void GetByOwner_NoMatches_ReturnsEmptyList()
    {
        Bank bank = new Bank("Альфа");
        bank.OpenAccount(Debit("D-001", "Иванов Иван", 100m));

        Assert.Empty(bank.GetByOwner("Сидоров"));
    }

    // ───────────── Активы / долги ─────────────

    [Fact]
    public void Totals_EmptyBank_AreZero()
    {
        // Граничный случай: пустая коллекция
        Bank bank = new Bank("Альфа");

        Assert.Equal(0m, bank.GetTotalAssets());
        Assert.Equal(0m, bank.GetTotalDebt());
    }

    [Fact]
    public void Totals_MixedAccounts_SeparatePositiveAndNegativeBalances()
    {
        // Arrange
        Bank bank = new Bank("Альфа");
        bank.OpenAccount(Debit("D-001", "Иванов", 1000m));
        bank.OpenAccount(Debit("D-002", "Петров", 500m));
        CreditAccount credit = Credit("C-001", "Сидоров");
        credit.Withdraw(300m); // баланс -300
        bank.OpenAccount(credit);

        // Act + Assert
        Assert.Equal(1500m, bank.GetTotalAssets());
        Assert.Equal(300m, bank.GetTotalDebt());
    }

    // ───────────── Статистика по типам ─────────────

    [Fact]
    public void CountByType_And_SumByType_ReturnValuesForRequestedTypeOnly()
    {
        // Arrange
        Bank bank = new Bank("Альфа");
        bank.OpenAccount(Debit("D-001", "Иванов", 1000m));
        bank.OpenAccount(Debit("D-002", "Петров", 500m));
        bank.OpenAccount(Credit("C-001", "Сидоров", 200m));

        // Act + Assert
        Assert.Equal(2, bank.CountByType("DebitAccount"));
        Assert.Equal(1500m, bank.SumByType("DebitAccount"));
        Assert.Equal(1, bank.CountByType("CreditAccount"));
        Assert.Equal(200m, bank.SumByType("CreditAccount"));
    }

    [Fact]
    public void CountByType_UnknownType_ReturnsZero()
    {
        Bank bank = new Bank("Альфа");
        bank.OpenAccount(Debit("D-001", "Иванов", 1000m));

        Assert.Equal(0, bank.CountByType("DepositAccount"));
        Assert.Equal(0m, bank.SumByType("DepositAccount"));
    }

    // ───────────── Сортировка / проценты ─────────────

    [Fact]
    public void GetSortedAccounts_ReturnsAscendingByBalance()
    {
        // Arrange
        Bank bank = new Bank("Альфа");
        bank.OpenAccount(Debit("D-001", "Иванов", 500m));
        bank.OpenAccount(Debit("D-002", "Петров", 100m));
        bank.OpenAccount(Debit("D-003", "Сидоров", 900m));

        // Act
        List<Account> sorted = bank.GetSortedAccounts();

        // Assert
        Assert.Equal(new[] { "D-002", "D-001", "D-003" }, sorted.Select(a => a.AccountNumber));
    }

    [Fact]
    public void ApplyMonthlyInterestToAll_AppliesRuleOfEachAccountType()
    {
        // Arrange
        Bank bank = new Bank("Альфа");
        DebitAccount debit = Debit("D-001", "Иванов", 1000m, 0.01m);   // +1% → 1010
        CreditAccount creditDebt = Credit("C-001", "Петров");
        creditDebt.Withdraw(1000m);                            // долг 1000, +2% → -1020
        CreditAccount creditPlus = Credit("C-002", "Сидоров", 500m);     // нет долга → без изменений
        bank.OpenAccount(debit);
        bank.OpenAccount(creditDebt);
        bank.OpenAccount(creditPlus);

        // Act
        bank.ApplyMonthlyInterestToAll();

        // Assert
        Assert.Equal(1010m, debit.Balance);
        Assert.Equal(-1020m, creditDebt.Balance);
        Assert.Equal(500m, creditPlus.Balance);
    }

    [Fact]
    public void ToString_ContainsNameAndAccountCount()
    {
        Bank bank = new Bank("Альфа");
        bank.OpenAccount(Debit("D-001", "Иванов", 100m));

        string text = bank.ToString();

        Assert.Contains("Альфа", text);
        Assert.Contains("1", text);
    }
}
