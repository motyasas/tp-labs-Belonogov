using System;

namespace lab2;

// Депозитный счёт: снять деньги можно только после окончания срока и только всю сумму.
public class DepositAccount : Account
{
    public DateTime MaturityDate { get; }
    public decimal MonthlyRate { get; }
    public bool IsClosed { get; private set; }

    public DepositAccount(string accountNumber, string ownerName, decimal initialBalance,
                          decimal monthlyRate, DateTime maturityDate)
        : base(accountNumber, ownerName, initialBalance)
    {
        if (initialBalance <= 0)
        {
            throw new ArgumentException("Начальная сумма депозита должна быть положительной.");
        }
        if (monthlyRate <= 0 || monthlyRate >= 1)
        {
            throw new ArgumentException("Месячная ставка должна быть от 0 до 1.");
        }
        if (maturityDate <= DateTime.Today)
        {
            throw new ArgumentException("Дата окончания должна быть в будущем.");
        }

        MonthlyRate = monthlyRate;
        MaturityDate = maturityDate;
        IsClosed = false;
    }

    public override decimal Withdraw(decimal amount)
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("Вклад уже закрыт.");
        }
        if (DateTime.Today < MaturityDate)
        {
            throw new InvalidOperationException("Вклад нельзя закрыть раньше " + MaturityDate.ToShortDateString());
        }
        if (amount != Balance)
        {
            throw new InvalidOperationException("С депозита можно снять только всю сумму: " + Balance.ToString("C"));
        }

        decimal withdrawn = Balance;
        Balance = 0;
        IsClosed = true;
        return withdrawn;
    }

    public override void ApplyInterest()
    {
        if (IsClosed == false)
        {
            Balance = Balance + Balance * MonthlyRate;
        }
    }

    public override string ToString()
    {
        string text = base.ToString() + " | " + MonthlyRate.ToString("P1") + "/мес, до " + MaturityDate.ToShortDateString();
        if (IsClosed)
        {
            text = text + " [ЗАКРЫТ]";
        }
        return text;
    }
}