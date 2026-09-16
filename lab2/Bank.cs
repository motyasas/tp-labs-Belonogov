namespace lab2;

/// <summary>
/// Класс-менеджер: хранит все счета и предоставляет статистику.
/// </summary>
public class Bank
{
    private readonly List<Account> _accounts = [];

    public string Name { get; }

    public Bank(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название банка не может быть пустым.", nameof(name));
        Name = name;
    }

    /// <summary>Открыть новый счёт.</summary>
    public void OpenAccount(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);
        if (_accounts.Any(a => a.AccountNumber == account.AccountNumber))
            throw new ArgumentException($"Счёт №{account.AccountNumber} уже существует.");
        _accounts.Add(account);
    }

    /// <summary>Найти счёт по номеру.</summary>
    public Account? FindByNumber(string accountNumber) =>
        _accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);

    /// <summary>Все счета указанного владельца.</summary>
    public IEnumerable<Account> GetByOwner(string ownerName) =>
        _accounts.Where(a => a.OwnerName.Contains(ownerName, StringComparison.OrdinalIgnoreCase));

    /// <summary>Суммарные активы (сумма всех положительных балансов).</summary>
    public decimal TotalAssets =>
        _accounts.Where(a => a.Balance > 0).Sum(a => a.Balance);

    /// <summary>Суммарный долг (сумма всех отрицательных балансов, возвращается как положительное число).</summary>
    public decimal TotalDebt =>
        _accounts.Where(a => a.Balance < 0).Sum(a => -a.Balance);

    /// <summary>Применить начисление процентов ко всем счетам.</summary>
    public void ApplyMonthlyInterestToAll()
    {
        foreach (var account in _accounts)
            account.ApplyInterest();
    }

    /// <summary>Счета, сгруппированные по типу, с суммами.</summary>
    public IEnumerable<(string Type, int Count, decimal TotalBalance)> GetStatsByType() =>
        _accounts
            .GroupBy(a => a.GetType().Name)
            .Select(g => (Type: g.Key, Count: g.Count(), TotalBalance: g.Sum(a => a.Balance)))
            .OrderByDescending(x => x.TotalBalance);

    /// <summary>Отсортированный список счетов по балансу.</summary>
    public List<Account> GetSortedAccounts()
    {
        var sorted = new List<Account>(_accounts);
        sorted.Sort(); // IComparable<Account>
        return sorted;
    }

    public override string ToString() => $"Банк «{Name}» | счетов: {_accounts.Count}";
}
