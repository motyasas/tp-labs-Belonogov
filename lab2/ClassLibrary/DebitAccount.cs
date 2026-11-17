using System;

namespace lab2;

// Дебетовый счёт: в минус уйти нельзя, каждый месяц начисляется кэшбэк.
public class DebitAccount : Account
{
    public decimal CashbackRate { get; }

    public DebitAccount(string accountNumber, string ownerName, decimal initialBalance, decimal cashbackRate)
        : base(accountNumber, ownerName, initialBalance)
    {
        if (cashbackRate < 0 || cashbackRate >= 1)
        {
            throw new ArgumentException("Ставка кэшбэка должна быть от 0 до 1.");
        }
        CashbackRate = cashbackRate;
    }

    public override decimal Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Сумма снятия должна быть положительной.");
        }
        if (amount > Balance)
        {
            throw new InvalidOperationException("Недостаточно средств. Доступно: " + Balance.ToString("C"));
        }

        Balance = Balance - amount;
        return amount;
    }

    public override void ApplyInterest()
    {
        Balance = Balance + Balance * CashbackRate;
    }

    public override string ToString()
    {
        return base.ToString() + " | кэшбэк " + CashbackRate.ToString("P1");
    }
}