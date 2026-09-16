using lab2;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ─── Создание банка и счетов ──────────────────────────────────────────────────
var bank = new Bank("Альфа-Банк");

var debit1 = new DebitAccount("D-001", "Иванов Иван", initialBalance: 50_000m, cashbackRate: 0.005m);
var debit2 = new DebitAccount("D-002", "Петрова Мария", initialBalance: 12_000m, cashbackRate: 0.01m);
var credit1 = new CreditAccount("C-001", "Сидоров Алексей", creditLimit: 100_000m, monthlyRate: 0.02m, initialBalance: 5_000m);
var credit2 = new CreditAccount("C-002", "Козлова Елена", creditLimit: 50_000m, monthlyRate: 0.025m);
var deposit1 = new DepositAccount("DEP-001", "Иванов Иван",
                  initialBalance: 200_000m, monthlyRate: 0.008m,
                  maturityDate: DateTime.Today.AddMonths(12));

bank.OpenAccount(debit1);
bank.OpenAccount(debit2);
bank.OpenAccount(credit1);
bank.OpenAccount(credit2);
bank.OpenAccount(deposit1);

// ─── Вывод всех счетов (полиморфизм: List<Account>, ToString()) ───────────────
Console.WriteLine($"\n=== {bank} ===");
Console.WriteLine("\n-- Все счета при открытии --");
foreach (Account acc in bank.GetSortedAccounts())
    Console.WriteLine(acc);

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

// Попытка снять больше лимита — поймаем исключение
Console.WriteLine("\n-- Попытка превысить кредитный лимит --");
try
{
    credit1.Withdraw(200_000m);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}

// Попытка снять депозит досрочно
Console.WriteLine("\n-- Попытка досрочно закрыть депозит --");
try
{
    deposit1.Withdraw(deposit1.Balance);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}

// ─── Начисление процентов (полиморфизм: каждый тип по-своему) ─────────────────
Console.WriteLine("\n-- После начисления месячных процентов --");
bank.ApplyMonthlyInterestToAll();
foreach (Account acc in bank.GetSortedAccounts())
    Console.WriteLine(acc);

// ─── Поиск по владельцу ──────────────────────────────────────────────────────
Console.WriteLine("\n-- Счета Иванова --");
foreach (var acc in bank.GetByOwner("Иванов"))
    Console.WriteLine(acc);

// ─── Статистика банка (LINQ) ─────────────────────────────────────────────────
Console.WriteLine("\n-- Статистика по типам счетов --");
foreach (var (type, count, total) in bank.GetStatsByType())
    Console.WriteLine($"  {type,-18} x{count}  суммарно: {total:C}");

Console.WriteLine($"\nСуммарные активы банка: {bank.TotalAssets:C}");
Console.WriteLine($"Суммарный долг банка:   {bank.TotalDebt:C}");

// ─── Сортировка (IComparable<Account>) ───────────────────────────────────────
Console.WriteLine("\n-- Счета, отсортированные по балансу (от меньшего к большему) --");
foreach (var acc in bank.GetSortedAccounts())
    Console.WriteLine($"  {acc.AccountNumber,-10} {acc.OwnerName,-20} {acc.Balance,12:C}");
