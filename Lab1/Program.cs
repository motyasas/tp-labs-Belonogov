using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Lab1
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //Факториал 

            Console.Write("Введите неотрицательное целое число n: ");
            string input = Console.ReadLine();

            try
            {
                int n = int.Parse(input);

                if (n < 0)
                {
                    Console.WriteLine("Ошибка: число должно быть неотрицательным.");
                    return;
                }

                BigInteger factorial = 1;
                for (int i = 2; i <= n; i++)
                {
                    factorial *= i;
                }

                Console.WriteLine($"Факториал числа {n} равен: {factorial}");
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: введено не целое число.");
            }
            catch (OverflowException)
            {
                Console.WriteLine("Ошибка: число слишком большое.");
            }
        }
    }
}
