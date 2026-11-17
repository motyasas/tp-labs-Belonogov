namespace BankSystem.Core;

/// <summary>
/// Банк: хранит счета, проводит операции и записывает их в журнал.
/// В этом классе нет ни одного Console - он не знает, как его будут показывать пользователю.
/// </summary>
public class Bank
{
    private readonly List<Account> _accounts = new List<Account>();

    // Журнал операций. Тип - интерфейс, поэтому подойдёт любая реализация.
    private readonly ITransactionLog _log;

    public string Name { get; }

    public Bank(string name, ITransactionLog log)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название банка не может быть пустым.");
        }
        if (log == null)
        {
            throw new ArgumentException("Журнал операций не может быть null.");
        }
        Name = name;
        _log = log;
    }

    /// <summary>Количество открытых счетов.</summary>
    public int AccountCount
    {
        get { return _accounts.Count; }
    }

    // ---------------- Операции ----------------

    public void OpenAccount(Account account)
    {
        if (account == null)
        {
            throw new ArgumentException("Счёт не может быть null.");
        }
        if (FindByNumber(account.AccountNumber) != null)
        {
            throw new ArgumentException("Счёт №" + account.AccountNumber + " уже существует.");
        }

        _accounts.Add(account);
        _log.Write($"Открыт счёт {account.AccountNumber} ({account.OwnerName}), баланс {account.Balance}");
    }

    /// <summary>Пополнить счёт с указанным номером.</summary>
    public void Deposit(string accountNumber, decimal amount)
    {
        Account account = GetAccount(accountNumber);
        account.Deposit(amount); // если сумма некорректная, здесь будет исключение и в журнал ничего не попадёт
        _log.Write($"Пополнение {accountNumber} на {amount}, баланс {account.Balance}");
    }

    /// <summary>Снять деньги со счёта с указанным номером. Возвращает снятую сумму.</summary>
    public decimal Withdraw(string accountNumber, decimal amount)
    {
        Account account = GetAccount(accountNumber);
        decimal withdrawn = account.Withdraw(amount);
        _log.Write($"Снятие {accountNumber}: {withdrawn}, баланс {account.Balance}");
        return withdrawn;
    }

    /// <summary>
    /// Начислить месячные проценты на все счета.
    /// Каждый тип счёта считает проценты по-своему (полиморфизм): мы просто вызываем ApplyInterest.
    /// </summary>
    public void ApplyMonthlyInterestToAll()
    {
        foreach (Account account in _accounts)
        {
            account.ApplyInterest();
        }
        _log.Write("Начислены месячные проценты на все счета");
    }

    /// <summary>История операций из журнала.</summary>
    public List<string> GetHistory()
    {
        return _log.ReadAll();
    }

    // ---------------- Поиск и статистика ----------------

    /// <summary>Найти счёт по номеру. Если не найден - вернёт null.</summary>
    public Account? FindByNumber(string accountNumber)
    {
        foreach (Account account in _accounts)
        {
            if (account.AccountNumber == accountNumber)
            {
                return account;
            }
        }
        return null;
    }

    /// <summary>Все счета, отсортированные по балансу (сравнение задано в Account.CompareTo).</summary>
    public List<Account> GetSortedAccounts()
    {
        List<Account> sorted = new List<Account>(_accounts);
        sorted.Sort();
        return sorted;
    }

    /// <summary>Сумма всех положительных балансов.</summary>
    public decimal GetTotalAssets()
    {
        decimal total = 0;
        foreach (Account account in _accounts)
        {
            if (account.Balance > 0)
            {
                total = total + account.Balance;
            }
        }
        return total;
    }

    /// <summary>Сумма всех долгов (как положительное число).</summary>
    public decimal GetTotalDebt()
    {
        decimal total = 0;
        foreach (Account account in _accounts)
        {
            if (account.Balance < 0)
            {
                total = total - account.Balance;
            }
        }
        return total;
    }

    // Как FindByNumber, но если счёта нет - сразу исключение (для операций)
    private Account GetAccount(string accountNumber)
    {
        Account? account = FindByNumber(accountNumber);
        if (account == null)
        {
            throw new ArgumentException("Счёт №" + accountNumber + " не найден.");
        }
        return account;
    }

    public override string ToString()
    {
        return "Банк «" + Name + "» | счетов: " + _accounts.Count;
    }
}
