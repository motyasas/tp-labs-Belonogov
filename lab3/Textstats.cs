using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace lab3
{
    /// <summary>
    /// Результаты статистики текстового файла.
    /// Снаружи значения можно только читать (private set),
    /// менять их может только сам класс через метод AddLine.
    /// </summary>
    public class TextStatistics
    {
        public string FilePath { get; }
        public long FileSizeBytes { get; }
        public long LineCount { get; private set; }
        public long WordCount { get; private set; }
        public long CharCountWithSpaces { get; private set; }
        public long CharCountWithoutSpaces { get; private set; }
        public string LongestWord { get; private set; } = "";

        // Знаки препинания, которые отрезаем с краёв слова ("привет," -> "привет")
        private static readonly char[] Punctuation =
        {
            '.', ',', '!', '?', ':', ';', '"', '\'', '«', '»', '(', ')', '-', '—', '…'
        };

        public TextStatistics(string filePath, long fileSizeBytes)
        {
            FilePath = filePath;
            FileSizeBytes = fileSizeBytes;
        }

        /// <summary>
        /// Учитывает в статистике одну строку файла.
        /// </summary>
        public void AddLine(string line)
        {
            LineCount++;
            CharCountWithSpaces += line.Length;

            // Символы без пробелов: считаем всё, что не пробел и не табуляция
            foreach (char c in line)
            {
                if (!char.IsWhiteSpace(c))
                {
                    CharCountWithoutSpaces++;
                }
            }

            // Split(null) режет строку по любым пробельным символам,
            // RemoveEmptyEntries убирает пустые куски (если подряд несколько пробелов)
            string[] tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            foreach (string token in tokens)
            {
                string word = token.Trim(Punctuation);
                if (word.Length == 0)
                {
                    continue; // это был просто знак препинания, например "—"
                }

                WordCount++;
                if (word.Length > LongestWord.Length)
                {
                    LongestWord = word;
                }
            }
        }
    }

    /// <summary>
    /// Логика анализа текстового файла.
    /// Отдельный класс — ничего не знает про консоль.
    /// </summary>
    public static class TextAnalyzer
    {
        /// <summary>
        /// Синхронный потоковый анализ через StreamReader.
        /// Файл читается по одной строке, поэтому в память целиком не загружается —
        /// так можно обработать файл любого размера (бонус 2).
        /// </summary>
        public static TextStatistics AnalyzeStream(string path, Encoding encoding)
        {
            FileInfo info = new FileInfo(path);
            TextStatistics stats = new TextStatistics(path, info.Length);

            // using: файл гарантированно закроется в конце блока, даже при ошибке
            using (StreamReader reader = new StreamReader(path, encoding))
            {
                string? line = reader.ReadLine();
                while (line != null)
                {
                    stats.AddLine(line);
                    line = reader.ReadLine();
                }
            }

            return stats;
        }

        /// <summary>
        /// Асинхронный анализ (бонус 1): файл читается целиком через ReadAllTextAsync.
        /// Пока диск читает файл, поток программы не стоит в ожидании (await его освобождает).
        /// Минус: весь текст оказывается в памяти, для огромных файлов лучше AnalyzeStream.
        /// </summary>
        public static async Task<TextStatistics> AnalyzeAsync(string path, Encoding encoding)
        {
            string text = await File.ReadAllTextAsync(path, encoding);

            FileInfo info = new FileInfo(path);
            TextStatistics stats = new TextStatistics(path, info.Length);

            // StringReader умеет ReadLine() так же, как StreamReader, только читает из строки.
            // Так строки делятся точно по тем же правилам, что и в потоковой версии.
            using (StringReader reader = new StringReader(text))
            {
                string? line = reader.ReadLine();
                while (line != null)
                {
                    stats.AddLine(line);
                    line = reader.ReadLine();
                }
            }

            return stats;
        }
    }
}
