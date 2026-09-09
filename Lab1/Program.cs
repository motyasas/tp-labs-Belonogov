using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;

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

        }
    }
}