using System.Diagnostics;
using System.Text;

const int N = 30_000;

Stopwatch swTotal = Stopwatch.StartNew();

// --- 1. Генерация данных ---
Random rnd = new Random(42);                    // фиксированное зерно: результат воспроизводим
List<string> buyers = new List<string>();
List<int> amounts = new List<int>();
for (int i = 0; i < N; i++)
{
    buyers.Add("user" + rnd.Next(0, 5000));
    amounts.Add(rnd.Next(1, 10_000));
}

// --- 2. Список уникальных покупателей ---
Stopwatch sw = Stopwatch.StartNew();
List<string> unique = new List<string>();
// Оптимизация: List.Contains просматривает весь список (O(n)), и в цикле получается O(n^2).
// HashSet ищет элемент по хешу почти мгновенно (O(1)).
// HashSet.Add возвращает true, если элемента ещё не было - тогда добавляем его и в список
// (список нужен, чтобы сохранить порядок покупателей, как в исходной версии).
HashSet<string> seen = new HashSet<string>();
for (int i = 0; i < buyers.Count; i++)
{
    if (seen.Add(buyers[i]))
        unique.Add(buyers[i]);
}
Console.WriteLine($"Уникальных покупателей: {unique.Count}  ({sw.ElapsedMilliseconds} мс)");

// --- 3. Текстовый отчёт ---
sw.Restart();
string report = "";
for (int i = 0; i < buyers.Count; i++)
{
    report += buyers[i] + ";" + amounts[i] + Environment.NewLine;
}
Console.WriteLine($"Отчёт: {report.Length} символов  ({sw.ElapsedMilliseconds} мс)");

// --- 4. Сортировка сумм ---
sw.Restart();
List<int> sorted = new List<int>(amounts);
for (int i = 0; i < sorted.Count - 1; i++)
    for (int j = 0; j < sorted.Count - i - 1; j++)
        if (sorted[j] > sorted[j + 1])
            (sorted[j], sorted[j + 1]) = (sorted[j + 1], sorted[j]);
Console.WriteLine($"Медианная сумма: {sorted[sorted.Count / 2]}  ({sw.ElapsedMilliseconds} мс)");

// --- 5. Сумма покупок каждого покупателя ---
sw.Restart();
List<(string Buyer, long Total)> totals = new List<(string Buyer, long Total)>();
foreach (string b in unique)
{
    long total = 0;
    for (int i = 0; i < buyers.Count; i++)
        if (buyers[i] == b)
            total += amounts[i];
    totals.Add((b, total));
}
Console.WriteLine($"Максимум потратил: {totals.MaxBy(t => t.Total).Buyer}  ({sw.ElapsedMilliseconds} мс)");

Console.WriteLine($"ИТОГО: {swTotal.ElapsedMilliseconds} мс");
