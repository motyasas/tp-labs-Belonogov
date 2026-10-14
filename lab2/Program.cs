using lab2;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ─── Создание банка и счетов ──────────────────────────────────────────────────
Bank bank = new("Альфа-Банк");

DebitAccount debit1 = new("D-001", "Иванов Иван", initialBalance: 50_000m, cashbackRate: 0.005m);
DebitAccount debit2 = new("D-002", "Петрова Мария", initialBalance: 12_000m, cashbackRate: 0.01m);
CreditAccount credit1 = new("C-001", "Сидоров Алексей", creditLimit: 100_000m, monthlyRate: 0.02m, initialBalance: 5_000m);
CreditAccount credit2 = new("C-002", "Козлова Елена", creditLimit: 50_000m, monthlyRate: 0.025m);
DepositAccount deposit1 = new("DEP-001", "Иванов Иван",
    initialBalance: 200_000m, monthlyRate: 0.008m, maturityDate: DateTime.Today.AddMonths(12));

foreach (Account account in new Account[] { debit1, debit2, credit1, credit2, deposit1 })
    bank.OpenAccount(account);

// ─── Вывод всех счетов ───────────────────────────────────────────────────────
Console.WriteLine($"\n=== {bank} ===");
PrintAccounts("Все счета при открытии", bank.GetSortedAccounts());

// ─── Пополнения ──────────────────────────────────────────────────────────────
Console.WriteLine("\n-- Пополнения --");
debit1.Deposit(10_000m);
credit2.Deposit(20_000m);
Console.WriteLine($"Деб. {debit1.AccountNumber} после пополнения: {debit1.Balance:C}");
Console.WriteLine($"Кред. {credit2.AccountNumber} после пополнения: {credit2.Balance:C}");

// ─── Снятия ──────────────────────────────────────────────────────────────────
Console.WriteLine("\n-- Снятия --");
decimal withdrawn = debit1.Withdraw(5_000m);
Console.WriteLine($"Снято с {debit1.AccountNumber}: {withdrawn:C}, остаток: {debit1.Balance:C}");

withdrawn = credit1.Withdraw(30_000m); // уходим в минус
Console.WriteLine($"Снято с {credit1.AccountNumber}: {withdrawn:C}, баланс: {credit1.Balance:C}");

Console.WriteLine("\n-- Попытка превысить кредитный лимит --");
TryRun(() => credit1.Withdraw(200_000m));

Console.WriteLine("\n-- Попытка досрочно закрыть депозит --");
TryRun(() => deposit1.Withdraw(deposit1.Balance));

// ─── Начисление процентов ────────────────────────────────────────────────────
bank.ApplyMonthlyInterestToAll();
PrintAccounts("После начисления месячных процентов", bank.GetSortedAccounts());

// ─── Поиск по владельцу ──────────────────────────────────────────────────────
PrintAccounts("Счета Иванова", bank.GetByOwner("Иванов"));

// ─── Статистика банка ────────────────────────────────────────────────────────
Console.WriteLine("\n-- Статистика по типам счетов --");
foreach ((string type, int count, decimal total) in bank.GetStatsByType())
    Console.WriteLine($"  {type,-18} x{count}  суммарно: {total:C}");

Console.WriteLine($"\nСуммарные активы банка: {bank.TotalAssets:C}");
Console.WriteLine($"Суммарный долг банка:   {bank.TotalDebt:C}");

// ─── Сортировка по балансу ───────────────────────────────────────────────────
Console.WriteLine("\n-- Счета, отсортированные по балансу (от меньшего к большему) --");
foreach (Account account in bank.GetSortedAccounts())
    Console.WriteLine($"  {account.AccountNumber,-10} {account.OwnerName,-20} {account.Balance,12:C}");

// ─── Вспомогательные методы ──────────────────────────────────────────────────
static void PrintAccounts(string title, IEnumerable<Account> accounts)
{
    Console.WriteLine($"\n-- {title} --");
    foreach (Account account in accounts)
        Console.WriteLine(account);
}

static void TryRun(Action action)
{
    try
    {
        action();
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}