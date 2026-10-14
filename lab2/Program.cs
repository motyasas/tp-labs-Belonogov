using System;
using System.Collections.Generic;

namespace lab2;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // ─── Создание банка и счетов ─────────────────────────────────────
        Bank bank = new Bank("Альфа-Банк");

        DebitAccount debit1 = new DebitAccount("D-001", "Иванов Иван", 50000m, 0.005m);
        DebitAccount debit2 = new DebitAccount("D-002", "Петрова Мария", 12000m, 0.01m);
        CreditAccount credit1 = new CreditAccount("C-001", "Сидоров Алексей", 5000m, 100000m, 0.02m);
        CreditAccount credit2 = new CreditAccount("C-002", "Козлова Елена", 0m, 50000m, 0.025m);
        DepositAccount deposit1 = new DepositAccount("DEP-001", "Иванов Иван", 200000m, 0.008m,
                                                     DateTime.Today.AddMonths(12));

        bank.OpenAccount(debit1);
        bank.OpenAccount(debit2);
        bank.OpenAccount(credit1);
        bank.OpenAccount(credit2);
        bank.OpenAccount(deposit1);

        // ─── Вывод всех счетов ───────────────────────────────────────────
        Console.WriteLine();
        Console.WriteLine("=== " + bank + " ===");
        Console.WriteLine();
        Console.WriteLine("-- Все счета при открытии --");
        PrintAccounts(bank.GetSortedAccounts());

        // ─── Пополнения ──────────────────────────────────────────────────
        Console.WriteLine();
        Console.WriteLine("-- Пополнения --");
        debit1.Deposit(10000m);
        credit2.Deposit(20000m);
        Console.WriteLine("Деб. " + debit1.AccountNumber + " после пополнения: " + debit1.Balance.ToString("C"));
        Console.WriteLine("Кред. " + credit2.AccountNumber + " после пополнения: " + credit2.Balance.ToString("C"));

        // ─── Снятия ──────────────────────────────────────────────────────
        Console.WriteLine();
        Console.WriteLine("-- Снятия --");
        decimal withdrawn = debit1.Withdraw(5000m);
        Console.WriteLine("Снято с " + debit1.AccountNumber + ": " + withdrawn.ToString("C")
            + ", остаток: " + debit1.Balance.ToString("C"));

        withdrawn = credit1.Withdraw(30000m); // уходим в минус
        Console.WriteLine("Снято с " + credit1.AccountNumber + ": " + withdrawn.ToString("C")
            + ", баланс: " + credit1.Balance.ToString("C"));

        // ─── Попытка превысить кредитный лимит ───────────────────────────
        Console.WriteLine();
        Console.WriteLine("-- Попытка превысить кредитный лимит --");
        try
        {
            credit1.Withdraw(200000m);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }

        // ─── Попытка досрочно закрыть депозит ────────────────────────────
        Console.WriteLine();
        Console.WriteLine("-- Попытка досрочно закрыть депозит --");
        try
        {
            deposit1.Withdraw(deposit1.Balance);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }

        // ─── Начисление процентов ────────────────────────────────────────
        bank.ApplyMonthlyInterestToAll();
        Console.WriteLine();
        Console.WriteLine("-- После начисления месячных процентов --");
        PrintAccounts(bank.GetSortedAccounts());

        // ─── Поиск по владельцу ──────────────────────────────────────────
        Console.WriteLine();
        Console.WriteLine("-- Счета Иванова --");
        PrintAccounts(bank.GetByOwner("Иванов"));

        // ─── Статистика по типам счетов ──────────────────────────────────
        Console.WriteLine();
        Console.WriteLine("-- Статистика по типам счетов --");
        string[] types = { "DebitAccount", "CreditAccount", "DepositAccount" };
        foreach (string type in types)
        {
            Console.WriteLine("  " + type + " x" + bank.CountByType(type)
                + "  суммарно: " + bank.SumByType(type).ToString("C"));
        }

        Console.WriteLine();
        Console.WriteLine("Суммарные активы банка: " + bank.GetTotalAssets().ToString("C"));
        Console.WriteLine("Суммарный долг банка:   " + bank.GetTotalDebt().ToString("C"));
    }

    // Выводит список счетов на экран
    static void PrintAccounts(List<Account> accounts)
    {
        foreach (Account account in accounts)
        {
            Console.WriteLine(account);
        }
    }
}