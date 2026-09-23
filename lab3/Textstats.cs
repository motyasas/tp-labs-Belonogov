using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab3
{
    /// <summary>
    /// Результаты статистики текстового файла
    /// </summary>
    public record TextStatistics(
        long LineCount,
        long WordCount,
        long CharCountWithSpaces,
        long CharCountWithoutSpaces,
        string LongestWord,
        string FilePath,
        long FileSizeBytes
    );

    /// <summary>
    /// Логика анализа текстового файла.
    /// Отдельный класс — не зависит от консольного UI.
    /// </summary>
    public static class TextAnalyzer
    {
        // Синхронный вариант (потоковый, не грузит весь файл в память) 

        /// <summary>
        /// Потоковая обработка файла через StreamReader.
        /// Работает с файлами любого размера — не использует ReadAllText.
        /// </summary>
        public static TextStatistics AnalyzeStream(string path, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;

            long lines = 0, words = 0, charsWith = 0, charsWithout = 0;
            string longestWord = "";

            using var reader = new StreamReader(path, encoding);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                lines++;
                charsWith += line.Length;

                // Разбиваем по любым пробельным символам
                var tokens = line.Split((char[]?)null,
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var token in tokens)
                {
                    // Убираем знаки препинания по краям слова
                    string word = token.Trim('.', ',', '!', '?', ':', ';',
                                            '"', '\'', '«', '»', '(', ')',
                                            '-', '—', '…');
                    if (word.Length == 0) continue;

                    words++;
                    charsWithout += word.Length;

                    if (word.Length > longestWord.Length)
                        longestWord = word;
                }
            }

            var info = new FileInfo(path);
            return new TextStatistics(lines, words, charsWith, charsWithout,
                                      longestWord, path, info.Length);
        }

        //  Асинхронный вариант (бонус) 

    }
}