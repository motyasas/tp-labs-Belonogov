using System.Globalization;
using System.Text;
using BankSystem.Core;
using ConsoleTables;

// Консольный интерфейс банка. Здесь только меню, ввод и вывод.
// Вся логика - в библиотеке BankSystem.Core.

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Выбираем, куда писать журнал операций. Банку всё равно - он работает через интерфейс.
ITransactionLog log;
Console.Write("Хранить журнал операций в файле bank_log.txt? (д/н, Enter = д): ");
string? logAnswer = Console.ReadLine();
if (logAnswer != null && logAnswer.Trim().ToLower() == "н")
{
    log = new MemoryTransactionLog();
    Console.WriteLine("Журнал хранится в памяти.");
}
else
{
    log = new FileTransactionLog("bank_log.txt");
    Console.WriteLine("Журнал пишется в файл bank_log.txt.");
}

Bank bank = new Bank("Учебный банк", log);

bool running = true;
while (running)
{
    Console.WriteLine();
    Console.WriteLine("===== " + bank + " =====");
    Console.WriteLine("1 - Открыть дебетовый счёт");
    Console.WriteLine("2 - Открыть кредитный счёт");
    Console.WriteLine("3 - Пополнить счёт");
    Console.WriteLine("4 - Снять деньги");
    Console.WriteLine("5 - Начислить месячные проценты");
    Console.WriteLine("6 - Показать все счета");
    Console.WriteLine("7 - Показать журнал операций");
    Console.WriteLine("0 - Выход");
    Console.Write("Выбор: ");

    string? choice = Console.ReadLine();

    // Классы банка сообщают об ошибках исключениями (например, "недостаточно средств").
    // Ловим их здесь и показываем пользователю понятное сообщение - программа не падает.
    try
    {
        switch (choice)
        {
            case "1":
                OpenDebit(bank);
                break;
            case "2":
                OpenCredit(bank);
                break;
            case "3":
                DepositMoney(bank);
                break;
            case "4":
                WithdrawMoney(bank);
                break;
            case "5":
                bank.ApplyMonthlyInterestToAll();
                Console.WriteLine("Проценты начислены.");
                break;
            case "6":
                ShowAccounts(bank);
                break;
            case "7":
                ShowHistory(bank);
                break;
            case "0":
                running = false;
                break;
            case null:
                running = false; // ввод закончился
                break;
            default:
                Console.WriteLine("Нет такого пункта меню.");
                break;
        }
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine("Ошибка: " + ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine("Операция невозможна: " + ex.Message);
    }
    catch (IOException ex)
    {
        Console.WriteLine("Ошибка работы с файлом журнала: " + ex.Message);
    }
}

// ---------------- Пункты меню ----------------

static void OpenDebit(Bank bank)
{
    string number = AskText("Номер счёта: ");
    string owner = AskText("Владелец: ");
    decimal balance = AskDecimal("Начальный баланс: ");
    decimal cashback = AskDecimal("Кэшбэк в месяц (например 0,01 = 1%): ");

    bank.OpenAccount(new DebitAccount(number, owner, balance, cashback));
    Console.WriteLine("Счёт открыт.");
}

static void OpenCredit(Bank bank)
{
    string number = AskText("Номер счёта: ");
    string owner = AskText("Владелец: ");
    decimal balance = AskDecimal("Начальный баланс: ");
    decimal limit = AskDecimal("Кредитный лимит: ");
    decimal rate = AskDecimal("Ставка в месяц (например 0,02 = 2%): ");

    bank.OpenAccount(new CreditAccount(number, owner, balance, limit, rate));
    Console.WriteLine("Счёт открыт.");
}

static void DepositMoney(Bank bank)
{
    string number = AskText("Номер счёта: ");
    decimal amount = AskDecimal("Сумма пополнения: ");
    bank.Deposit(number, amount);
    Console.WriteLine("Готово.");
}

static void WithdrawMoney(Bank bank)
{
    string number = AskText("Номер счёта: ");
    decimal amount = AskDecimal("Сумма снятия: ");
    decimal withdrawn = bank.Withdraw(number, amount);
    Console.WriteLine($"Снято: {withdrawn:C}");
}

static void ShowAccounts(Bank bank)
{
    // ConsoleTables - NuGet-пакет, рисует красивую таблицу в консоли.
    // Создаём таблицу с заголовками колонок и добавляем по строке на каждый счёт.
    ConsoleTable table = new ConsoleTable("Тип", "Номер", "Владелец", "Баланс");
    foreach (Account account in bank.GetSortedAccounts())
    {
        table.AddRow(account.GetType().Name, account.AccountNumber, account.OwnerName, account.Balance.ToString("C"));
    }
    table.Write(Format.Default);

    Console.WriteLine($"Активы: {bank.GetTotalAssets():C}   Долги: {bank.GetTotalDebt():C}");
}

static void ShowHistory(Bank bank)
{
    List<string> history = bank.GetHistory();
    if (history.Count == 0)
    {
        Console.WriteLine("Журнал пуст.");
        return;
    }
    foreach (string record in history)
    {
        Console.WriteLine(record);
    }
}

// ---------------- Ввод с проверкой ----------------

// Спрашивает текст, пока не введут непустую строку
static string AskText(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (input == null)
        {
            throw new ArgumentException("ввод прерван.");
        }
        if (input.Trim() != "")
        {
            return input.Trim();
        }
        Console.WriteLine("Значение не может быть пустым.");
    }
}

// Спрашивает число, пока не введут корректное. Подходит и запятая, и точка.
static decimal AskDecimal(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (input == null)
        {
            throw new ArgumentException("ввод прерван.");
        }

        input = input.Replace(',', '.');
        if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
        {
            return value;
        }
        Console.WriteLine("Нужно ввести число.");
    }
}
