using lab3;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// Лабораторная работа 3, вариант 6: статистика текстового файла.
// Здесь только меню и ввод-вывод, вся логика подсчёта - в Textstats.cs.

// Регистрируем провайдер кодировок (без этого Windows-1251 и CP866 недоступны)
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;

bool running = true;
while (running)
{
    PrintMenu();
    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await RunAnalysis(false);
            break;
        case "2":
            await RunAnalysis(true);
            break;
        case "3":
            ShowEncodingHelp();
            break;
        case "0":
            running = false;
            Console.WriteLine("До свидания!");
            break;
        default:
            Console.WriteLine("Неизвестная команда. Введите число от 0 до 3.");
            break;
    }
}

// ---------------- Методы меню ----------------

static void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("----------- Меню -----------");
    Console.WriteLine("  1  Анализ файла (потоковый, StreamReader)");
    Console.WriteLine("  2  Анализ файла (асинхронный, ReadAllTextAsync)");
    Console.WriteLine("  3  Справка по кодировкам");
    Console.WriteLine("  0  Выход");
    Console.Write("Выбор: ");
}

// useAsync = false -> потоковое чтение, true -> асинхронное
static async Task RunAnalysis(bool useAsync)
{
    string? path = AskFilePath();
    if (path == null)
    {
        return;
    }

    if (Directory.Exists(path))
    {
        Console.WriteLine("Ошибка: указана папка, а нужен файл.");
        return;
    }

    Encoding? encoding = AskEncoding();
    if (encoding == null)
    {
        return;
    }

    // Все возможные ошибки работы с файлом ловим здесь, чтобы программа не "падала".
    // Важно: более частные исключения (FileNotFound) пишем раньше общего (IOException),
    // иначе до них дело не дойдёт.
    try
    {
        TextStatistics stats;
        if (useAsync)
        {
            stats = await TextAnalyzer.AnalyzeAsync(path, encoding);
        }
        else
        {
            stats = TextAnalyzer.AnalyzeStream(path, encoding);
        }

        PrintStats(stats);
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine($"Ошибка: файл не найден - {path}");
    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine($"Ошибка: папка не найдена - {path}");
    }
    catch (UnauthorizedAccessException)
    {
        Console.WriteLine($"Ошибка: нет прав на чтение файла (или это папка) - {path}");
    }
    catch (IOException ex)
    {
        Console.WriteLine($"Ошибка ввода-вывода: {ex.Message}");
    }
}

static void ShowEncodingHelp()
{
    Console.WriteLine();
    Console.WriteLine("Поддерживаемые кодировки:");
    Console.WriteLine("  utf8  - UTF-8 (по умолчанию, современный стандарт)");
    Console.WriteLine("  1251  - Windows-1251 (старые русскоязычные файлы Windows)");
    Console.WriteLine("  866   - CP866 / DOS");
    Console.WriteLine("Если текст отображается «кракозябрами» - попробуйте другую кодировку.");
}

// ---------------- Ввод данных ----------------

// Возвращает полный путь к файлу или null, если ввод некорректный
static string? AskFilePath()
{
    Console.Write("Путь к файлу: ");
    string? path = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(path))
    {
        Console.WriteLine("Путь не указан.");
        return null;
    }

    // Убираем пробелы и кавычки по краям (Проводник копирует путь в кавычках)
    path = path.Trim().Trim('"');

    try
    {
        // Относительный путь превращаем в полный (относительно текущей папки)
        return Path.GetFullPath(path);
    }
    catch (Exception)
    {
        // GetFullPath бросает исключение, если в пути недопустимые символы
        Console.WriteLine("Некорректный путь.");
        return null;
    }
}

// Возвращает выбранную кодировку или null, если ввели что-то непонятное
static Encoding? AskEncoding()
{
    Console.Write("Кодировка [utf8 / 1251 / 866, Enter = utf8]: ");
    string? input = Console.ReadLine();
    if (input == null)
    {
        return null;
    }

    switch (input.Trim().ToLower())
    {
        case "":
        case "utf8":
        case "utf-8":
            return Encoding.UTF8;
        case "1251":
            return Encoding.GetEncoding(1251);
        case "866":
            return Encoding.GetEncoding(866);
        default:
            Console.WriteLine("Неизвестная кодировка.");
            return null;
    }
}

// ---------------- Вывод результата ----------------

static void PrintStats(TextStatistics stats)
{
    Console.WriteLine();
    Console.WriteLine($"Файл                 : {Path.GetFileName(stats.FilePath)}");
    Console.WriteLine($"Размер               : {stats.FileSizeBytes} байт");
    Console.WriteLine($"Строк                : {stats.LineCount}");
    Console.WriteLine($"Слов                 : {stats.WordCount}");
    Console.WriteLine($"Символов с пробелами : {stats.CharCountWithSpaces}");
    Console.WriteLine($"Символов без пробелов: {stats.CharCountWithoutSpaces}");

    if (stats.LongestWord == "")
    {
        Console.WriteLine("Самое длинное слово : (в файле нет слов)");
    }
    else
    {
        Console.WriteLine($"Самое длинное слово : {stats.LongestWord} ({stats.LongestWord.Length} букв)");
    }
}
