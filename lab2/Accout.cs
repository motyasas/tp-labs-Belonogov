namespace lab2;

/// <summary>
/// Абстрактный базовый класс банковского счёта.
/// </summary>
public abstract class Account : IComparable<Account>
{
    private decimal _balance;

    public string AccountNumber { get; }
    public string OwnerName { get; }

    public decimal Balance
    {
        get => _balance;
        protected set => _balance = value;
    }

    protected Account(string accountNumber, string ownerName, decimal initialBalance = 0)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Номер счёта не может быть пустым.", nameof(accountNumber));
        if (string.IsNullOrWhiteSpace(ownerName))
            throw new ArgumentException("Имя владельца не может быть пустым.", nameof(ownerName));
        if (initialBalance < 0)
            throw new ArgumentOutOfRangeException(nameof(initialBalance), "Начальный баланс не может быть отрицательным.");

        AccountNumber = accountNumber;
        OwnerName = ownerName;
        _balance = initialBalance;
    }

    /// <summary>Пополнить счёт.</summary>
    public virtual void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Сумма пополнения должна быть положительной.");
        _balance += amount;
    }

    /// <summary>Снять деньги со счёта. Возвращает фактически снятую сумму.</summary>
    public abstract decimal Withdraw(decimal amount);

    /// <summary>Начислить проценты/плату — конкретная логика у каждого типа счёта.</summary>
    public abstract void ApplyInterest();

    public int CompareTo(Account? other)
    {
        if (other is null) return 1;
        return Balance.CompareTo(other.Balance);
    }

    public override string ToString() =>
        $"[{GetType().Name}] #{AccountNumber} ({OwnerName}): {Balance:C}";
}
