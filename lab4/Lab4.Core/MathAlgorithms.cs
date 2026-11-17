using System.Numerics;

namespace Lab4.Core;

/// <summary>Алгоритмы из лабораторной 1, вынесенные из консольного ввода-вывода.</summary>
public static class MathAlgorithms
{
    /// <summary>n! через BigInteger — без ограничения n &lt;= 20.</summary>
    public static BigInteger Factorial(int n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "n должно быть неотрицательным.");

        BigInteger result = BigInteger.One;
        for (int i = 2; i <= n; i++)
            result *= i;
        return result;
    }

    /// <summary>Числа Фибоначчи F(0), F(1), ... не превосходящие n (включительно).</summary>
    public static long[] FibonacciUpTo(long n)
    {
        if (n < 0)
            throw new ArgumentOutOfRangeException(nameof(n), "n должно быть неотрицательным.");

        List<long> list = new List<long>();
        long a = 0;
        long b = 1;

        while (a <= n)
        {
            list.Add(a);

            // Исправление ошибки: раньше a + b молча переполнялось у границы long.
            if (b > long.MaxValue - a)
            {
                // следующее число после b не помещается в long, но сам b ещё может подойти
                if (b <= n)
                    list.Add(b);
                break;
            }

            long next = a + b;
            a = b;
            b = next;
        }

        return list.ToArray();
    }

    /// <summary>A = sin(5/x) * ch(sqrt(x - 1)) + e^(5x). Область определения: x >= 1.</summary>
    public static double FunctionA(double x)
    {
        if (double.IsNaN(x) || x < 1)
            throw new ArgumentOutOfRangeException(nameof(x), "Функция определена только при x >= 1.");

        double result = Math.Sin(5.0 / x) * Math.Cosh(Math.Sqrt(x - 1)) + Math.Exp(5 * x);

        if (!double.IsFinite(result))
            throw new OverflowException("Результат не помещается в double.");

        return result;
    }

    /// <summary>
    /// sin(x) рядом Тейлора: x - x^3/3! + x^5/5! - ...
    /// Складываем члены, пока очередной член по модулю больше eps.
    /// Каждый следующий член получаем из предыдущего умножением на -x^2 / ((2k)(2k+1)).
    /// </summary>
    public static double SinTaylor(double x, double eps)
    {
        if (eps <= 0)
            throw new ArgumentOutOfRangeException(nameof(eps), "Точность должна быть положительной.");
        if (!double.IsFinite(x))
            throw new ArgumentOutOfRangeException(nameof(x), "x должен быть конечным числом.");

        // приводим x к отрезку [-pi; pi], синус от этого не меняется
        x = Math.IEEERemainder(x, 2 * Math.PI);

        double term = x;
        double sum = 0;
        int k = 1;
        while (Math.Abs(term) > eps)
        {
            sum = sum + term;
            term = term * (-x * x) / ((2 * k) * (2 * k + 1));
            k++;
        }
        return sum;
    }
}
