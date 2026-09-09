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
            /// Вычисляет n! через BigInteger — без ограничения на n ≤ 20.
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


            //===================================================
            //===================================================

            /// <summary>
            /// Возвращает последовательность чисел Фибоначчи от F(0) до первого F(k) >= n 
            /// включительно, но не выходящего за n. Если n = 0 — только «0».
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
        }
    }
}