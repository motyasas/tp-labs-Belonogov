namespace lab2;

/// <summary>
/// Кредитный счёт: разрешён уход в минус до кредитного лимита;
/// на отрицательный остаток ежемесячно начисляется процент.
/// </summary>
public class CreditAccount : Account
{
    /// <summary>Максимально допустимый долг (положительное число).</summary>
    public decimal CreditLimit { get; }

    /// <summary>Месячная процентная ставка на долг (например 0.02 = 2%).</summary>
    public decimal MonthlyRate { get; }

    public CreditAccount(string accountNumber, string ownerName,
                         decimal creditLimit, decimal monthlyRate,
                         decimal initialBalance = 0)
        : base(accountNumber, ownerName, initialBalance)
    {
        if (creditLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(creditLimit), "Кредитный лимит должен быть положительным.");
        if (monthlyRate <= 0 || monthlyRate >= 1)
            throw new ArgumentOutOfRangeException(nameof(monthlyRate), "Месячная ставка должна быть в диапазоне (0, 1).");

        CreditLimit = creditLimit;
        MonthlyRate = monthlyRate;
    }

    /// <summary>
    /// Снять деньги. Допускается уход в минус, но не глубже кредитного лимита.
    /// </summary>
    public override decimal Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Сумма снятия должна быть положительной.");
        if (Balance - amount < -CreditLimit)
            throw new InvalidOperationException(
                $"Превышен кредитный лимит {CreditLimit:C}. Доступно к снятию: {Balance + CreditLimit:C}.");

        Balance -= amount;
        return amount;
    }

    /// <summary>Начислить проценты на долг (если баланс отрицательный).</summary>
    public override void ApplyInterest()
    {
        if (Balance < 0)
            Balance += Balance * MonthlyRate; // баланс ещё более отрицательный
    }

    public override string ToString() =>
        $"{base.ToString()} | лимит {CreditLimit:C}, ставка {MonthlyRate:P0}/мес";
}
