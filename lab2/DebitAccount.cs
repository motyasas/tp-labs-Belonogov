namespace lab2;

/// <summary>
/// Дебетовый счёт: нельзя уйти в минус; ежемесячный кэшбэк на остаток.
/// </summary>
public class DebitAccount : Account
{
    /// <summary>Ставка кэшбэка (доля от остатка, например 0.005 = 0.5%).</summary>
    public decimal CashbackRate { get; }

    public DebitAccount(string accountNumber, string ownerName,
                        decimal initialBalance = 0, decimal cashbackRate = 0.005m)
        : base(accountNumber, ownerName, initialBalance)
    {
        if (cashbackRate < 0 || cashbackRate >= 1)
            throw new ArgumentOutOfRangeException(nameof(cashbackRate), "Ставка кэшбэка должна быть в диапазоне [0, 1).");
        CashbackRate = cashbackRate;
    }

    /// <summary>Снять деньги. Нельзя снять больше, чем есть на счёте.</summary>
    public override decimal Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Сумма снятия должна быть положительной.");
        if (amount > Balance)
            throw new InvalidOperationException($"Недостаточно средств. Доступно: {Balance:C}.");

        Balance -= amount;
        return amount;
    }

    /// <summary>Начислить кэшбэк на текущий остаток.</summary>
    public override void ApplyInterest()
    {
        decimal cashback = Balance * CashbackRate;
        Balance += cashback;
    }

    public override string ToString() =>
        $"{base.ToString()} | кэшбэк {CashbackRate:P1}";
}
