using System;
using System.Collections.Generic;

namespace lab2;

// Банк хранит список всех счетов и умеет считать по ним статистику.
public class Bank
{
    private List<Account> _accounts = new List<Account>();

    public string Name { get; }

    public Bank(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название банка не может быть пустым.");
        }
        Name = name;
    }

    // Открыть новый счёт
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
    }

    // Найти счёт по номеру (если не найден — вернёт null)
    public Account FindByNumber(string accountNumber)
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

    // Все счета, у которых имя владельца содержит заданный текст
    public List<Account> GetByOwner(string ownerName)
    {
        List<Account> result = new List<Account>();
        foreach (Account account in _accounts)
        {
            if (account.OwnerName.Contains(ownerName))
            {
                result.Add(account);
            }
        }
        return result;
    }

    // Сумма всех положительных балансов
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

    // Сумма всех долгов (как положительное число)
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

    // Начислить проценты на всех счетах
    // (каждый счёт делает это по-своему — это и есть полиморфизм)
    public void ApplyMonthlyInterestToAll()
    {
        foreach (Account account in _accounts)
        {
            account.ApplyInterest();
        }
    }

    // Сколько счетов указанного типа (например "DebitAccount")
    public int CountByType(string typeName)
    {
        int count = 0;
        foreach (Account account in _accounts)
        {
            if (account.GetType().Name == typeName)
            {
                count++;
            }
        }
        return count;
    }

    // Сумма балансов счетов указанного типа
    public decimal SumByType(string typeName)
    {
        decimal total = 0;
        foreach (Account account in _accounts)
        {
            if (account.GetType().Name == typeName)
            {
                total = total + account.Balance;
            }
        }
        return total;
    }

    // Список счетов, отсортированный по балансу
    public List<Account> GetSortedAccounts()
    {
        List<Account> sorted = new List<Account>(_accounts);
        sorted.Sort();
        return sorted;
    }

    public override string ToString()
    {
        return "Банк «" + Name + "» | счетов: " + _accounts.Count;
    }
}