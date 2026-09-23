namespace lab2;

/// <summary>
/// Депозитный счёт: снятие возможно только после окончания срока вклада;
/// ежемесячно начисляются проценты.
/// </summary>
public class DepositAccount : Account
{
    /// <summary>Дата окончания вклада.</summary>
    public DateTime MaturityDate { get; }

    /// <summary>Месячная процентная ставка (например 0.01 = 1%).</summary>
    public decimal MonthlyRate { get; }

    /// <summary>Вклад закрыт (деньги сняты)?</summary>
    public bool IsClosed { get; private set; }

    public DepositAccount(string accountNumber, string ownerName,
                          decimal initialBalance, decimal monthlyRate, DateTime maturityDate)
        : base(accountNumber, ownerName, initialBalance)
    {
        if (initialBalance <= 0)
            throw new ArgumentOutOfRangeException(nameof(initialBalance), "Начальная сумма депозита должна быть положительной.");
        if (monthlyRate <= 0 || monthlyRate >= 1)
            throw new ArgumentOutOfRangeException(nameof(monthlyRate), "Месячная ставка должна быть в диапазоне (0, 1).");
        if (maturityDate <= DateTime.Today)
            throw new ArgumentOutOfRangeException(nameof(maturityDate), "Дата окончания должна быть в будущем.");

        MonthlyRate = monthlyRate;
        MaturityDate = maturityDate;
    }

    /// <summary>
    /// Снять деньги возможно только после даты окончания и только всю сумму целиком.
    /// </summary>
    public override decimal Withdraw(decimal amount)
    {
        if (IsClosed)
            throw new InvalidOperationException("Вклад уже закрыт.");
        if (DateTime.Today < MaturityDate)
            throw new InvalidOperationException(
                $"Вклад нельзя закрыть раньше {MaturityDate:d}. Осталось {(MaturityDate - DateTime.Today).Days} дн.");
        if (amount != Balance)
            throw new ArgumentException($"С депозита можно снять только всю сумму: {Balance:C}.");

        decimal withdrawn = Balance;
        Balance = 0;
        IsClosed = true;
        return withdrawn;
    }

    /// <summary>Начислить проценты на остаток.</summary>
    public override void ApplyInterest()
    {
        if (!IsClosed)
            Balance += Balance * MonthlyRate;
    }

    public override string ToString() =>
        $"{base.ToString()} | {MonthlyRate:P0}/мес, до {MaturityDate:d}" +
        (IsClosed ? " [ЗАКРЫТ]" : "");
}
