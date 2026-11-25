using System;

namespace lab2;

// Кредитный счёт: можно уходить в минус, но не глубже кредитного лимита.
// На долг каждый месяц начисляются проценты.
public class CreditAccount : Account
{
    public decimal CreditLimit { get; }
    public decimal MonthlyRate { get; }

    public CreditAccount(string accountNumber, string ownerName, decimal initialBalance,
                         decimal creditLimit, decimal monthlyRate)
        : base(accountNumber, ownerName, initialBalance)
    {
        if (creditLimit <= 0)
        {
            throw new ArgumentException("Кредитный лимит должен быть положительным.");
        }
        if (monthlyRate <= 0 || monthlyRate >= 1)
        {
            throw new ArgumentException("Месячная ставка должна быть от 0 до 1.");
        }

        CreditLimit = creditLimit;
        MonthlyRate = monthlyRate;
    }

    public override decimal Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Сумма снятия должна быть положительной.");
        }
        if (Balance - amount < -CreditLimit)
        {
            throw new InvalidOperationException("Превышен кредитный лимит " + CreditLimit.ToString("C")
                + ". Доступно к снятию: " + (Balance + CreditLimit).ToString("C"));
        }

        Balance = Balance - amount;
        return amount;
    }

    public override void ApplyInterest()
    {
        // Проценты начисляются только на долг (отрицательный баланс)
        if (Balance < 0)
        {
            Balance = Balance + Balance * MonthlyRate;
        }
    }

    public override string ToString()
    {
        return base.ToString() + " | лимит " + CreditLimit.ToString("C") + ", ставка " + MonthlyRate.ToString("P1") + "/мес";
    }
}