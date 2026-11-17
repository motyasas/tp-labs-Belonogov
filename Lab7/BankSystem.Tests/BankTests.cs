using BankSystem.Core;

namespace BankSystem.Tests;

// Тесты пишутся против библиотеки Core, а не против консольного приложения:
// логика не зависит от консоли, поэтому её можно проверить напрямую.
// Журнал везде - MemoryTransactionLog, чтобы тесты не создавали файлы.
public class BankTests
{
    // Вспомогательный метод: банк с одним дебетовым счётом D-1 на 1000
    private static Bank CreateBankWithDebit(MemoryTransactionLog log)
    {
        Bank bank = new Bank("Тест", log);
        bank.OpenAccount(new DebitAccount("D-1", "Иванов", 1000m, 0.01m));
        return bank;
    }

    [Fact]
    public void OpenAccount_NewAccount_IncreasesCountAndWritesLog()
    {
        // Arrange
        MemoryTransactionLog log = new MemoryTransactionLog();
        Bank bank = new Bank("Тест", log);

        // Act
        bank.OpenAccount(new DebitAccount("D-1", "Иванов", 500m, 0.01m));

        // Assert
        Assert.Equal(1, bank.AccountCount);
        Assert.Single(log.ReadAll());
        Assert.Contains("D-1", log.ReadAll()[0]);
    }

    [Fact]
    public void OpenAccount_DuplicateNumber_Throws()
    {
        // Arrange
        Bank bank = CreateBankWithDebit(new MemoryTransactionLog());

        // Act + Assert
        Assert.Throws<ArgumentException>(
            () => bank.OpenAccount(new DebitAccount("D-1", "Петров", 100m, 0.01m)));
    }

    [Fact]
    public void Deposit_ExistingAccount_IncreasesBalance()
    {
        // Arrange
        MemoryTransactionLog log = new MemoryTransactionLog();
        Bank bank = CreateBankWithDebit(log);

        // Act
        bank.Deposit("D-1", 250m);

        // Assert
        Assert.Equal(1250m, bank.FindByNumber("D-1")!.Balance);
        Assert.Equal(2, log.ReadAll().Count); // открытие + пополнение
    }

    [Fact]
    public void Deposit_UnknownAccount_Throws()
    {
        Bank bank = CreateBankWithDebit(new MemoryTransactionLog());

        Assert.Throws<ArgumentException>(() => bank.Deposit("NO-SUCH", 100m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Deposit_NonPositiveAmount_ThrowsAndDoesNotWriteLog(double amount)
    {
        // Arrange
        MemoryTransactionLog log = new MemoryTransactionLog();
        Bank bank = CreateBankWithDebit(log);

        // Act + Assert
        Assert.Throws<ArgumentException>(() => bank.Deposit("D-1", (decimal)amount));
        Assert.Single(log.ReadAll()); // только запись об открытии счёта
    }

    [Theory]
    [InlineData(100, 900)]
    [InlineData(1000, 0)]   // граничный случай: снимаем всё до нуля
    public void Withdraw_DebitWithinBalance_DecreasesBalance(double amount, double expectedBalance)
    {
        // Arrange
        Bank bank = CreateBankWithDebit(new MemoryTransactionLog());

        // Act
        decimal withdrawn = bank.Withdraw("D-1", (decimal)amount);

        // Assert
        Assert.Equal((decimal)amount, withdrawn);
        Assert.Equal((decimal)expectedBalance, bank.FindByNumber("D-1")!.Balance);
    }

    [Fact]
    public void Withdraw_DebitMoreThanBalance_Throws()
    {
        Bank bank = CreateBankWithDebit(new MemoryTransactionLog());

        Assert.Throws<InvalidOperationException>(() => bank.Withdraw("D-1", 1000.01m));
    }

    [Fact]
    public void Withdraw_CreditWithinLimit_GoesNegative()
    {
        // Arrange
        Bank bank = new Bank("Тест", new MemoryTransactionLog());
        bank.OpenAccount(new CreditAccount("C-1", "Петров", 0m, 5000m, 0.02m));

        // Act
        bank.Withdraw("C-1", 3000m);

        // Assert
        Assert.Equal(-3000m, bank.FindByNumber("C-1")!.Balance);
        Assert.Equal(3000m, bank.GetTotalDebt());
    }

    [Fact]
    public void ApplyMonthlyInterestToAll_DifferentAccounts_EachAppliesOwnRule()
    {
        // Arrange
        Bank bank = new Bank("Тест", new MemoryTransactionLog());
        bank.OpenAccount(new DebitAccount("D-1", "Иванов", 1000m, 0.01m));     // +1% -> 1010
        bank.OpenAccount(new CreditAccount("C-1", "Петров", 0m, 5000m, 0.02m));
        bank.Withdraw("C-1", 1000m);                                           // долг 1000, +2% -> -1020

        // Act
        bank.ApplyMonthlyInterestToAll();

        // Assert
        Assert.Equal(1010m, bank.FindByNumber("D-1")!.Balance);
        Assert.Equal(-1020m, bank.FindByNumber("C-1")!.Balance);
    }

    [Fact]
    public void GetTotalAssets_EmptyBank_ReturnsZero()
    {
        Bank bank = new Bank("Тест", new MemoryTransactionLog());

        Assert.Equal(0m, bank.GetTotalAssets());
    }

    [Fact]
    public void Constructor_NullLog_Throws()
    {
        // null! - говорим компилятору "знаем, что тут null, так и задумано"
        Assert.Throws<ArgumentException>(() => new Bank("Тест", null!));
    }
}
