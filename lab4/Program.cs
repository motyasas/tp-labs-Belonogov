using System.Text;
using Lab4.Core;

namespace lab4
{
    // Небольшая демонстрация: вся логика лежит в библиотеке Lab4.Core,
    // а здесь только вызовы и вывод на экран. Главное в этой работе - тесты (проект Lab4.Tests).
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("25! = " + MathAlgorithms.Factorial(25));
            Console.WriteLine("Фибоначчи до 100: " + string.Join(", ", MathAlgorithms.FibonacciUpTo(100)));
            Console.WriteLine("A(2) = " + MathAlgorithms.FunctionA(2));
            Console.WriteLine("sin(1) рядом Тейлора = " + MathAlgorithms.SinTaylor(1, 1e-6) + ", Math.Sin = " + Math.Sin(1));

            Bank bank = new Bank("Учебный банк");
            bank.OpenAccount(new DebitAccount("D-001", "Иванов Иван", 1000m, 0.01m));
            bank.OpenAccount(new CreditAccount("C-001", "Петров Пётр", 0m, 5000m, 0.02m));
            bank.ApplyMonthlyInterestToAll();

            Console.WriteLine(bank);
            foreach (Account account in bank.GetSortedAccounts())
            {
                Console.WriteLine(account);
            }

            Console.WriteLine();
            Console.WriteLine("Запуск тестов: dotnet test (в папке lab4)");
        }
    }
}
