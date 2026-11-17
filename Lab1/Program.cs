using System.Globalization;
using System.Numerics;
using System.Text;

namespace Lab1
{
    internal class Program
    {
        // Точность для ряда Тейлора (задание 4): 10^-6
        const double Epsilon = 1e-6;

        static void Main(string[] args)
        {
            // Чтобы русские буквы нормально выводились в консоли
            Console.OutputEncoding = Encoding.UTF8;

            // Меню: крутимся в цикле, пока пользователь не выберет 0
            bool running = true;
            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("===== Лабораторная работа 1 =====");
                Console.WriteLine("1 - Факториал числа n");
                Console.WriteLine("2 - Числа Фибоначчи от 0 до n");
                Console.WriteLine("3 - Функция A = sin(5/x) * ch(sqrt(x-1)) + e^(5x)");
                Console.WriteLine("4 - sin(x) рядом Тейлора");
                Console.WriteLine("0 - Выход");
                Console.Write("Ваш выбор: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        RunTask1();
                        break;
                    case "2":
                        RunTask2();
                        break;
                    case "3":
                        RunTask3();
                        break;
                    case "4":
                        RunTask4();
                        break;
                    case "0":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Нет такого пункта меню. Введите число от 0 до 4.");
                        break;
                }
            }
        }

        // ================= Задание 1. Факториал =================

        // Здесь только ввод-вывод, само вычисление - в методе Factorial
        static void RunTask1()
        {
            Console.Write("Введите целое неотрицательное число n: ");
            string? input = Console.ReadLine();

            if (!int.TryParse(input, out int n) || n < 0)
            {
                Console.WriteLine("Ошибка: нужно ввести целое неотрицательное число.");
                return;
            }

            Console.WriteLine($"{n}! = {Factorial(n)}");
        }

        /// <summary>
        /// Вычисляет n! (бонус: через BigInteger, поэтому ограничения n <= 20 нет).
        /// BigInteger - это целое число "любой длины", оно не переполняется как long.
        /// </summary>
        static BigInteger Factorial(int n)
        {
            BigInteger result = BigInteger.One;
            for (int i = 2; i <= n; i++)
            {
                result = result * i;
            }
            return result;
        }

        // ================= Задание 2. Числа Фибоначчи =================

        static void RunTask2()
        {
            Console.Write("Введите верхнюю границу n (целое >= 0): ");
            string? input = Console.ReadLine();

            if (!long.TryParse(input, out long n) || n < 0)
            {
                Console.WriteLine("Ошибка: нужно ввести целое неотрицательное число.");
                return;
            }

            List<long> numbers = FibonacciUpTo(n);

            // string.Join склеивает элементы списка в одну строку через ", "
            Console.WriteLine(string.Join(", ", numbers));
        }

        /// <summary>
        /// Возвращает числа Фибоначчи 0, 1, 1, 2, 3, 5, ... которые не больше n.
        /// </summary>
        static List<long> FibonacciUpTo(long n)
        {
            List<long> result = new List<long>();

            long current = 0; // текущее число
            long next = 1;    // следующее число

            while (current <= n)
            {
                result.Add(current);

                // Если следующее сложение не влезет в long - дальше считать нельзя.
                // (условие "next > long.MaxValue - current" значит "current + next > long.MaxValue")
                if (next > long.MaxValue - current)
                {
                    if (next <= n)
                    {
                        result.Add(next);
                    }
                    break;
                }

                long sum = current + next;
                current = next;
                next = sum;
            }

            return result;
        }

        // ================= Задание 3. Функция =================

        static void RunTask3()
        {
            Console.Write("Введите значение x: ");
            if (!TryReadDouble(out double x))
            {
                Console.WriteLine("Ошибка: нужно ввести число.");
                return;
            }

            // Сначала проверяем, определена ли функция в этой точке
            string error = CheckDomain(x);
            if (error != "")
            {
                Console.WriteLine("Ошибка: " + error);
                return;
            }

            double a = CalculateA(x);

            // При больших x число e^(5x) не помещается даже в double (получается "бесконечность")
            if (double.IsInfinity(a))
            {
                Console.WriteLine("Ошибка: результат слишком большой, не помещается в double.");
                return;
            }

            Console.WriteLine($"A = {a}");
        }

        /// <summary>
        /// Проверяет область определения функции A.
        /// Возвращает пустую строку, если всё хорошо, иначе - текст ошибки.
        /// </summary>
        static string CheckDomain(double x)
        {
            if (x == 0)
            {
                return "функция не определена при x = 0 (деление на ноль в 5/x).";
            }
            if (x < 1)
            {
                return "функция не определена при x < 1 (отрицательное число под корнем sqrt(x - 1)).";
            }
            return "";
        }

        /// <summary>
        /// A = sin(5/x) * ch(sqrt(x - 1)) + e^(5x).
        /// ch - гиперболический косинус, в C# это Math.Cosh.
        /// </summary>
        static double CalculateA(double x)
        {
            double sinPart = Math.Sin(5.0 / x);
            double chPart = Math.Cosh(Math.Sqrt(x - 1));
            double expPart = Math.Exp(5 * x);

            return sinPart * chPart + expPart;
        }

        // ================= Задание 4. Ряд Тейлора для sin(x) =================

        static void RunTask4()
        {
            Console.Write("Введите x (в радианах): ");
            if (!TryReadDouble(out double x))
            {
                Console.WriteLine("Ошибка: нужно ввести число.");
                return;
            }

            double taylor = SinTaylor(x, Epsilon, out int termsCount);
            double library = Math.Sin(x);

            Console.WriteLine($"Ряд Тейлора:   sin({x}) = {taylor:F10}");
            Console.WriteLine($"Math.Sin:      sin({x}) = {library:F10}");
            Console.WriteLine($"Разница:       {Math.Abs(taylor - library):E2}");
            Console.WriteLine($"Сложено членов ряда: {termsCount}");
        }

        /// <summary>
        /// sin(x) = x - x^3/3! + x^5/5! - x^7/7! + ...
        /// Складываем члены, пока очередной член по модулю больше eps.
        /// Через out-параметр termsCount метод дополнительно "возвращает" количество сложенных членов.
        /// </summary>
        static double SinTaylor(double x, double eps, out int termsCount)
        {
            // sin - периодическая функция (период 2*pi). Для больших x члены ряда
            // становятся огромными и теряется точность, поэтому сначала
            // приводим x к отрезку [-pi; pi]. Значение синуса от этого не меняется.
            x = Math.IEEERemainder(x, 2 * Math.PI);

            double term = x;  // первый член ряда: x
            double sum = 0;
            termsCount = 0;

            int k = 1; // номер члена (нужен для формулы перехода)
            while (Math.Abs(term) > eps)
            {
                sum = sum + term;
                termsCount++;

                // БОНУС: следующий член получаем из предыдущего (рекуррентно),
                // а не считаем заново степень и факториал.
                //
                // Член номер k:     (-1)^k * x^(2k+1) / (2k+1)!
                // Член номер k+1:   отличается множителем  -x^2 / ((2k+2)(2k+3))
                //
                // Пример: x -> -x^3/6 :  x * (-x^2 / (2*3)) = -x^3/6
                //         -x^3/6 -> x^5/120 : (-x^3/6) * (-x^2 / (4*5)) = x^5/120
                //
                // Это быстрее (одно умножение и одно деление вместо целого факториала)
                // и точнее (не получаем огромных чисел вроде 30!, где теряется точность).
                term = term * (-x * x) / ((2 * k) * (2 * k + 1));
                k++;
            }

            return sum;
        }

        // ================= Вспомогательное =================

        /// <summary>
        /// Читает дробное число из консоли. Подходит и запятая, и точка (2,5 и 2.5).
        /// Возвращает true, если число прочитано успешно.
        /// </summary>
        static bool TryReadDouble(out double value)
        {
            string? input = Console.ReadLine();
            if (input == null)
            {
                value = 0;
                return false;
            }

            // Меняем запятую на точку и разбираем число в "инвариантной" культуре,
            // где разделитель дробной части - всегда точка.
            input = input.Replace(',', '.');
            return double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}
