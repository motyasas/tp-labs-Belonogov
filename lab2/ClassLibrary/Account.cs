using System;

namespace lab2;

// Абстрактный класс: от него нельзя создать объект напрямую,
// только от наследников (Debit, Credit, Deposit).
public abstract class Account : IComparable<Account>
{
    public string AccountNumber { get; }
    public string OwnerName { get; }
    public decimal Balance { get; protected set; }

    public Account(string accountNumber, string ownerName, decimal initialBalance)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new ArgumentException("Номер счёта не может быть пустым.");
        }
        if (string.IsNullOrWhiteSpace(ownerName))
        {
            throw new ArgumentException("Имя владельца не может быть пустым.");
        }
        if (initialBalance < 0)
        {
            throw new ArgumentException("Начальный баланс не может быть отрицательным.");
        }

        AccountNumber = accountNumber;
        OwnerName = ownerName;
        Balance = initialBalance;
    }

    // Пополнить счёт
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Сумма пополнения должна быть положительной.");
        }
        Balance = Balance + amount;
    }

    // Снять деньги: у каждого типа счёта своя логика
    public abstract decimal Withdraw(decimal amount);

    // Начислить проценты: у каждого типа счёта своя логика
    public abstract void ApplyInterest();

    // Нужен для сортировки по балансу
    public int CompareTo(Account other)
    {
        if (other == null)
        {
            return 1;
        }
        return Balance.CompareTo(other.Balance);
    }

    public override string ToString()
    {
        return "[" + GetType().Name + "] #" + AccountNumber + " (" + OwnerName + "): " + Balance.ToString("C");
    }
}