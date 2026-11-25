using System.Diagnostics;
using System.Text;
using Lab5;

// Лабораторная работа 5, вариант 1: число π по ряду Лейбница.
// Замеры делать в Release:  dotnet run -c Release

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine($"Ядер процессора: {Environment.ProcessorCount}");

// ---------- Ввод параметров ----------

Console.Write("Количество членов ряда (Enter = 100000000): ");
string? termsInput = Console.ReadLine();
long termsCount = 100_000_000;
if (!string.IsNullOrWhiteSpace(termsInput))
{
    if (!long.TryParse(termsInput, out termsCount) || termsCount <= 0)
    {
        Console.WriteLine("Ошибка: нужно целое положительное число.");
        return;
    }
}

Console.Write("Количество потоков (1..64): ");
if (!int.TryParse(Console.ReadLine(), out int threadCount) || threadCount < 1 || threadCount > 64)
{
    Console.WriteLine("Ошибка: нужно целое число от 1 до 64.");
    return;
}

// ---------- Три версии и замеры ----------

Stopwatch sw = Stopwatch.StartNew();
double piSequential = PiCalculator.Sequential(termsCount);
sw.Stop();
long timeSequential = sw.ElapsedMilliseconds;

sw.Restart();
double piThreads = PiCalculator.WithThreads(termsCount, threadCount);
sw.Stop();
long timeThreads = sw.ElapsedMilliseconds;

sw.Restart();
double piParallel = PiCalculator.WithParallelFor(termsCount, threadCount);
sw.Stop();
long timeParallel = sw.ElapsedMilliseconds;

Console.WriteLine();
Console.WriteLine($"Math.PI          = {Math.PI:F15}");
Console.WriteLine($"Последовательно  = {piSequential:F15}   ошибка {Math.Abs(piSequential - Math.PI):E2}   {timeSequential} мс");
Console.WriteLine($"Thread ({threadCount})       = {piThreads:F15}   ошибка {Math.Abs(piThreads - Math.PI):E2}   {timeThreads} мс   ускорение {Speedup(timeSequential, timeThreads):F2}x");
Console.WriteLine($"Parallel.For ({threadCount}) = {piParallel:F15}   ошибка {Math.Abs(piParallel - Math.PI):E2}   {timeParallel} мс   ускорение {Speedup(timeSequential, timeParallel):F2}x");

// Результаты дробных вычислений могут отличаться в последних знаках:
// потоки складывают числа в другом порядке, а у double ограниченная точность.
// Поэтому сравниваем не на точное равенство, а с маленьким допуском.
bool same = Math.Abs(piSequential - piThreads) < 1e-12 && Math.Abs(piSequential - piParallel) < 1e-12;
Console.WriteLine(same ? "Результаты всех версий совпадают." : "ВНИМАНИЕ: результаты различаются!");

// ---------- Таблица: время от числа потоков (для PR) ----------

Console.WriteLine();
Console.WriteLine("Таблица замеров (версия на Thread):");
Console.WriteLine("| Потоков | Время, мс | Ускорение |");
Console.WriteLine("|---------|-----------|-----------|");
Console.WriteLine($"| посл.   | {timeSequential,9} | {1.0,9:F2} |");

int[] threadVariants = { 1, 2, 4, 8, 16 };
foreach (int n in threadVariants)
{
    sw.Restart();
    PiCalculator.WithThreads(termsCount, n);
    sw.Stop();
    Console.WriteLine($"| {n,7} | {sw.ElapsedMilliseconds,9} | {Speedup(timeSequential, sw.ElapsedMilliseconds),9:F2} |");
}

// Ускорение = время последовательной версии / время параллельной
static double Speedup(long sequentialMs, long parallelMs)
{
    if (parallelMs == 0)
    {
        return 0; // слишком быстро, чтобы измерить
    }
    return (double)sequentialMs / parallelMs;
}
