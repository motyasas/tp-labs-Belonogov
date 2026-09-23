using lab3;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// Регистрируем провайдер кодировок (Windows-1251, CP866 и др.)
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

Console.OutputEncoding = Encoding.UTF8;


bool running = true;
while (running)
{
    PrintMenu();
    string? choice = Console.ReadLine()?.Trim();

    switch (choice)
    {
        case "1":
            RunStreamAnalysis();
            break;
        case "2":
            await RunAsyncAnalysis();
            break;
        case "3":
            ShowEncodingHelp();
            break;
        case "0":
            running = false;
            Console.WriteLine("\nДо свидания!");
            break;
        default:
            Console.WriteLine("Неизвестная команда. Введите число от 0 до 3.");
            break;
    }
}

//  Методы меню 

static void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("─── Меню ────────────────────────────────────────");
    Console.WriteLine("  1  Анализ файла  (потоковый, StreamReader)");
    Console.WriteLine("  2  Анализ файла  (асинхронный, ReadAllTextAsync) [не реализовано]");
    Console.WriteLine("  3  Справка по кодировкам");
    Console.WriteLine("  0  Выход");
    Console.WriteLine("─────────────────────────────────────────────────");
    Console.Write("Выбор: ");
}

static void RunStreamAnalysis()
{
    string path = PromptFilePath();
    if (path == null) return;

    Encoding enc = PromptEncoding();

    Console.WriteLine("\nОбработка файла потоком (StreamReader)...");

    try
    {
        var stats = TextAnalyzer.AnalyzeStream(path, enc);
        PrintStats(stats, "Потоковый (StreamReader)");
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine($"Ошибка: файл не найден — {path}");
    }
    catch (IOException ex)
    {
        Console.WriteLine($"Ошибка ввода-вывода: {ex.Message}");
    }
}

static async Task RunAsyncAnalysis()
{
    //код
}

static void ShowEncodingHelp()
{
    Console.WriteLine();
    Console.WriteLine("Поддерживаемые кодировки:");
    Console.WriteLine("  utf8     — UTF-8 (по умолчанию, современный стандарт)");
    Console.WriteLine("  1251     — Windows-1251 (старые русскоязычные файлы Windows)");
    Console.WriteLine("  866      — CP866 / DOS (файлы из командной строки DOS/Windows)");
    Console.WriteLine();
    Console.WriteLine("Если текст отображается «кракозябрами» — попробуйте другую кодировку.");
}

//  Вспомогательные методы 

static string PromptFilePath()
{
    Console.Write("\nПуть к файлу: ");
    string? path = Console.ReadLine()?.Trim().Trim('"');

    if (string.IsNullOrEmpty(path))
    {
        Console.WriteLine("Путь не указан.");
        return null!;
    }

    // Если указано только имя — ищем рядом с exe
    if (!Path.IsPathRooted(path))
        path = Path.Combine(AppContext.BaseDirectory, path);

    return path;
}

static Encoding PromptEncoding()
{
    Console.Write("Кодировка [utf8 / 1251 / 866, Enter = utf8]: ");
    string? input = Console.ReadLine()?.Trim().ToLower();

    return input switch
    {
        "1251" => Encoding.GetEncoding(1251),
        "866" => Encoding.GetEncoding(866),
        _ => Encoding.UTF8
    };
}

static void PrintStats(TextStatistics s, string method)
{
    string size = s.FileSizeBytes < 1024
        ? $"{s.FileSizeBytes} Б"
        : s.FileSizeBytes < 1024 * 1024
            ? $"{s.FileSizeBytes / 1024.0:F1} КБ"
            : $"{s.FileSizeBytes / 1024.0 / 1024.0:F2} МБ";

    Console.WriteLine();
    Console.WriteLine($"╔══ Результат [{method}] ══");
    Console.WriteLine($"║  Файл               : {Path.GetFileName(s.FilePath)}");
    Console.WriteLine($"║  Размер             : {size}");
    Console.WriteLine($"║  Строк              : {s.LineCount,10}");
    Console.WriteLine($"║  Слов               : {s.WordCount,10}");
    Console.WriteLine($"║  Символов (с пробел): {s.CharCountWithSpaces,10}");
    Console.WriteLine($"║  Символов (без проб): {s.CharCountWithoutSpaces,10}");
    Console.WriteLine($"║  Самое длинное слово: {s.LongestWord} ({s.LongestWord.Length} букв)");
    Console.WriteLine($"╚{'═'.ToString().PadRight(50, '═')}");
}