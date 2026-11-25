namespace Lab5
{
    /// <summary>
    /// Вычисление числа π по ряду Лейбница:
    ///     π/4 = 1 - 1/3 + 1/5 - 1/7 + ...
    /// Член ряда номер k (k = 0, 1, 2, ...) равен (-1)^k / (2k + 1).
    /// Здесь только вычисления, никакого ввода-вывода.
    /// </summary>
    public static class PiCalculator
    {
        /// <summary>
        /// Сумма членов ряда с номерами от start (включительно) до end (НЕ включительно).
        /// Этот метод используют все версии: и последовательная, и параллельные.
        /// </summary>
        public static double SumRange(long start, long end)
        {
            double sum = 0;
            for (long k = start; k < end; k++)
            {
                // чётные члены со знаком +, нечётные со знаком -
                if (k % 2 == 0)
                {
                    sum = sum + 1.0 / (2 * k + 1);
                }
                else
                {
                    sum = sum - 1.0 / (2 * k + 1);
                }
            }
            return sum;
        }

        /// <summary>
        /// 1) Последовательная версия: один поток считает все члены подряд.
        /// </summary>
        public static double Sequential(long termsCount)
        {
            return 4 * SumRange(0, termsCount);
        }

        /// <summary>
        /// 2) Версия на классе Thread: диапазон членов делим на threadCount равных частей,
        /// каждую часть считает свой поток.
        /// </summary>
        public static double WithThreads(long termsCount, int threadCount)
        {
            double total = 0;              // общая сумма - её меняют все потоки
            object lockObj = new object(); // объект-"замок" для lock

            Thread[] threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                // Границы части для потока номер i
                long start = GetPartStart(termsCount, threadCount, i);
                long end = GetPartStart(termsCount, threadCount, i + 1);

                // Код, который выполнит поток. Записан как лямбда: () => { ... }
                // Переменные start и end объявлены внутри цикла, поэтому
                // у каждого потока свои собственные значения.
                threads[i] = new Thread(() =>
                {
                    // Сначала считаем свою часть в ЛОКАЛЬНУЮ переменную - здесь никто не мешает
                    double partSum = SumRange(start, end);

                    // А вот общую переменную total меняем только под lock:
                    // "total = total + partSum" - это чтение, сложение и запись.
                    // Если два потока сделают это одновременно, одно слагаемое потеряется
                    // (гонка данных). lock пускает внутрь только один поток за раз.
                    lock (lockObj)
                    {
                        total = total + partSum;
                    }
                });

                threads[i].Start(); // поток начинает работу параллельно с остальными
            }

            // Ждём, пока все потоки закончат
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }

            return 4 * total;
        }

        /// <summary>
        /// 3) Версия на Parallel.For: .NET сам раздаёт части потокам из пула потоков.
        /// Мы всё так же делим ряд на partsCount частей, а Parallel.For
        /// выполняет "тело" цикла для разных частей одновременно.
        /// </summary>
        public static double WithParallelFor(long termsCount, int partsCount)
        {
            double total = 0;
            object lockObj = new object();

            // MaxDegreeOfParallelism - сколько потоков максимум можно занять
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = partsCount;

            // Для каждого номера части i от 0 до partsCount-1 выполнится лямбда
            Parallel.For(0, partsCount, options, i =>
            {
                long start = GetPartStart(termsCount, partsCount, i);
                long end = GetPartStart(termsCount, partsCount, i + 1);

                double partSum = SumRange(start, end);

                lock (lockObj)
                {
                    total = total + partSum;
                }
            });

            return 4 * total;
        }

        /// <summary>
        /// Начало части номер partIndex, если termsCount членов делим на partsCount частей.
        /// Например, 10 членов на 3 части: 0..3, 3..6, 6..10 (последняя чуть больше).
        /// </summary>
        public static long GetPartStart(long termsCount, int partsCount, int partIndex)
        {
            return termsCount * partIndex / partsCount;
        }
    }
}
