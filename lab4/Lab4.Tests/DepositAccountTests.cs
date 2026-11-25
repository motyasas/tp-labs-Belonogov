using Lab4.Core;

namespace Lab4.Tests;

public class DepositAccountTests
{
    // Дата окончания вклада - через год от сегодняшнего дня.
    // Берём дату относительно "сегодня", чтобы тесты не сломались со временем.
    private static readonly DateTime Maturity = DateTime.Today.AddYears(1);

    // Вспомогательный метод: создаёт депозит на 1000 под 1% в месяц
    private static DepositAccount CreateDeposit()
    {
        return new DepositAccount("DEP-001", "Иванов Иван", 1000m, 0.01m, Maturity);
    }

    //  Конструктор / инварианты 

    [Fact]
    public void Constructor_ValidArguments_AccountIsOpen()
    {
        // Arrange + Act
        DepositAccount account = CreateDeposit();

        // Assert
        Assert.False(account.IsClosed);
        Assert.Equal(1000m, account.Balance);
        Assert.Equal(Maturity, account.MaturityDate);
    }

    [Fact]
    public void Constructor_MaturityDateToday_Throws()
    {
        // Граничный случай: «сегодня» — это ещё не «в будущем».
        // () => ... — это маленькая функция без имени (лямбда). Assert.Throws вызывает её
        // и проверяет, что внутри было выброшено исключение нужного типа.
        Assert.Throws<ArgumentException>(
            () => new DepositAccount("DEP-1", "Иванов", 1000m, 0.01m, DateTime.Today));
    }

    [Fact]
    public void Constructor_MaturityDateInPast_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new DepositAccount("DEP-1", "Иванов", 1000m, 0.01m, DateTime.Today.AddDays(-1)));
    }

    [Fact]
    public void Constructor_ZeroInitialAmount_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => new DepositAccount("DEP-1", "Иванов", 0m, 0.01m, Maturity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-0.1)]
    public void Constructor_InvalidRate_Throws(double rate)
    {
        // decimal нельзя передать в InlineData, поэтому передаём double и приводим
        Assert.Throws<ArgumentException>(
            () => new DepositAccount("DEP-1", "Иванов", 1000m, (decimal)rate, Maturity));
    }

    //  Снятие 

    [Fact]
    public void Withdraw_BeforeMaturity_ThrowsAndKeepsAccountOpen()
    {
        // Arrange
        DepositAccount account = CreateDeposit();

        // Act + Assert: обычный Withdraw берёт сегодняшнюю дату, а срок ещё не наступил
        Assert.Throws<InvalidOperationException>(() => account.Withdraw(account.Balance));
        Assert.False(account.IsClosed);
        Assert.Equal(1000m, account.Balance);
    }

    [Fact]
    public void WithdrawOnDate_OneDayBeforeMaturity_Throws()
    {
        // Граничный случай: накануне даты окончания
        DepositAccount account = CreateDeposit();

        Assert.Throws<InvalidOperationException>(
            () => account.WithdrawOnDate(1000m, Maturity.AddDays(-1)));
    }

    [Fact]
    public void WithdrawOnDate_OnMaturityDate_WholeAmount_ClosesAccount()
    {
        // Arrange
        DepositAccount account = CreateDeposit();

        // Act: ровно в день окончания срока
        decimal withdrawn = account.WithdrawOnDate(1000m, Maturity);

        // Assert
        Assert.Equal(1000m, withdrawn);
        Assert.Equal(0m, account.Balance);
        Assert.True(account.IsClosed);
    }

    [Fact]
    public void WithdrawOnDate_AfterMaturity_PartialAmount_Throws()
    {
        // Arrange
        DepositAccount account = CreateDeposit();

        // Act + Assert: снять можно только всю сумму
        Assert.Throws<InvalidOperationException>(
            () => account.WithdrawOnDate(500m, Maturity.AddDays(5)));
        Assert.False(account.IsClosed);
        Assert.Equal(1000m, account.Balance);
    }

    [Fact]
    public void WithdrawOnDate_AlreadyClosed_Throws()
    {
        // Arrange
        DepositAccount account = CreateDeposit();
        account.WithdrawOnDate(1000m, Maturity);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(
            () => account.WithdrawOnDate(1000m, Maturity));
    }

    //  Проценты и ToString 

    [Fact]
    public void ApplyInterest_OpenDeposit_AddsMonthlyInterest()
    {
        // Arrange: 1000 * 1% = 10
        DepositAccount account = CreateDeposit();

        // Act
        account.ApplyInterest();

        // Assert
        Assert.Equal(1010m, account.Balance);
    }

    [Fact]
    public void ToString_ClosedDeposit_ContainsClosedMark()
    {
        // Arrange
        DepositAccount account = CreateDeposit();
        account.WithdrawOnDate(1000m, Maturity);

        // Act
        string text = account.ToString();

        // Assert
        Assert.Contains("[ЗАКРЫТ]", text);
    }
}
