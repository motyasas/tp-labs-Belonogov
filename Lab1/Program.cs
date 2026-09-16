using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Lab1
{
    internal class Program
    {
        static void Main(string[] args)
        {

            /// <summary>
            /// Вычисляет n! через BigInteger — без ограничения на n <= 20.
            /// </summary>
            static BigInteger Factorial(int n)
            {
                BigInteger result = BigInteger.One;
                for (int i = 2; i <= n; i++)
                    result *= i;
                return result;
            }


            Console.Write("Введите целое неотрицательное число n: ");
            string? input = Console.ReadLine();

            if (!int.TryParse(input, out int n) || n < 0)
            {
                Console.WriteLine("Ошибка: нужно ввести целое неотрицательное число.");
                return;
            }

            Console.WriteLine($"{n}! = {Factorial(n)}");


            /// <summary>
            /// Возвращает последовательность чисел Фибоначчи от F(0) до первого F(k) >= n 
            /// включительно, но не выходящего за n. Если n = 0 — только "0".
            /// </summary>
            static long[] FibonacciUpTo(long n)
            {
                List<long> list = new List<long>();

                long a = 0;
                long b = 1;

                while (a <= n)
                {
                    list.Add(a);

                    long next = a + b;
                    a = b;
                    b = next;
                }

                return list.ToArray();
            }

            Console.Write("Введите верхнюю границу n (целое >= 0): ");
            string? input_scnd_N = Console.ReadLine();

            long n_scnd;

            if (!long.TryParse(input_scnd_N, out n_scnd) || n_scnd < 0)
            {
                Console.WriteLine("Ошибка: нужно ввести целое неотрицательное число.");
            }
            else
            {
                long[] fibs = FibonacciUpTo(n_scnd);
                Console.WriteLine(string.Join(", ", fibs));
            }

            // Задание 3: A = sin(5/x) * ch(sqrt(x - 1)) + e^(5x)
            // ch(t) — гиперболический косинус: (e^t + e^(-t)) / 2

            Console.Write("\nВведите значение x: ");
            string? input_x = Console.ReadLine();

            if (!double.TryParse(input_x, out double x))
            {
                Console.WriteLine("Ошибка: нужно ввести число.");
            }
            else
            {
                // Проверка: деление на ноль в sin(5/x)
                if (x == 0)
                {
                    Console.WriteLine("Ошибка: функция не определена при x = 0 (деление на ноль в 5/x).");
                }
                // Проверка: отрицательное число под корнем sqrt(x - 1)
                else if (x < 1)
                {
                    Console.WriteLine("Ошибка: функция не определена при x < 1 (отрицательное число под корнем √(x - 1)).");
                }
                else
                {
                    double sinPart = Math.Sin(5.0 / x);
                    double sqrtPart = Math.Sqrt(x - 1);
                    double chPart = Math.Cosh(sqrtPart);   // ch(t) = Math.Cosh(t)
                    double expPart = Math.Exp(5 * x);

                    double A = sinPart * chPart + expPart;

                    Console.WriteLine($"A = sin(5/x) * ch(√(x-1)) + e^(5x) = {A}");
                }
            }
        }
    }
}