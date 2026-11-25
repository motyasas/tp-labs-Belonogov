using System.Numerics;
using Lab4.Core;

namespace Lab4.Tests;

public class MathAlgorithmsTests
{
    //  Factorial 

    [Fact]
    public void Factorial_OfZero_ReturnsOne()
    {
        // Arrange + Act
        BigInteger result = MathAlgorithms.Factorial(0);

        // Assert
        Assert.Equal(BigInteger.One, result);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, 120)]
    [InlineData(10, 3628800)]
    [InlineData(20, 2432902008176640000)] // максимум, помещающийся в long
    public void Factorial_SmallN_ReturnsExpected(int n, long expected)
    {
        // Act
        BigInteger result = MathAlgorithms.Factorial(n);

        // Assert
        Assert.Equal(new BigInteger(expected), result);
    }

    [Fact]
    public void Factorial_OfTwentyOne_ExceedsLongRange()
    {
        // Act: 21! уже не помещается в long — BigInteger должен справиться
        BigInteger result = MathAlgorithms.Factorial(21);

        // Assert
        Assert.Equal(BigInteger.Parse("51090942171709440000"), result);
        Assert.True(result > long.MaxValue);
    }

    [Theory]
    [InlineData(25, "15511210043330985984000000")]
    [InlineData(30, "265252859812191058636308480000000")]
    public void Factorial_LargeN_ReturnsExactValue(int n, string expected)
    {
        // Act
        BigInteger result = MathAlgorithms.Factorial(n);

        // Assert
        Assert.Equal(BigInteger.Parse(expected), result);
    }

    [Fact]
    public void Factorial_Of100_Has158Digits()
    {
        // Act
        string digits = MathAlgorithms.Factorial(100).ToString();

        // Assert
        Assert.Equal(158, digits.Length);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Factorial_NegativeN_ThrowsArgumentOutOfRange(int n)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MathAlgorithms.Factorial(n));
    }

    //  FibonacciUpTo 

    [Fact]
    public void FibonacciUpTo_Zero_ReturnsOnlyZero()
    {
        // Act
        long[] result = MathAlgorithms.FibonacciUpTo(0);

        // Assert
        Assert.Equal(new long[] { 0 }, result);
    }

    [Fact]
    public void FibonacciUpTo_One_ReturnsZeroOneOne()
    {
        // Act: в последовательности два раза встречается 1 (F(1) и F(2))
        long[] result = MathAlgorithms.FibonacciUpTo(1);

        // Assert
        Assert.Equal(new long[] { 0, 1, 1 }, result);
    }

    [Theory]
    [InlineData(10, new long[] { 0, 1, 1, 2, 3, 5, 8 })]
    [InlineData(20, new long[] { 0, 1, 1, 2, 3, 5, 8, 13 })]
    [InlineData(21, new long[] { 0, 1, 1, 2, 3, 5, 8, 13, 21 })] // граница включается
    public void FibonacciUpTo_ReturnsExpectedSequence(long n, long[] expected)
    {
        // Act
        long[] result = MathAlgorithms.FibonacciUpTo(n);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FibonacciUpTo_NegativeN_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MathAlgorithms.FibonacciUpTo(-1));
    }

    [Fact]
    public void FibonacciUpTo_LongMaxValue_DoesNotOverflow()
    {
        // Регрессионный тест на найденную ошибку: раньше a + b переполнялось
        // и цикл выдавал отрицательные «числа Фибоначчи» (и мог не завершиться).

        // Act
        long[] result = MathAlgorithms.FibonacciUpTo(long.MaxValue);

        // Assert: F(0)..F(92) — ровно 93 числа, последнее F(92) = 7540113804746346429
        Assert.Equal(93, result.Length);
        Assert.Equal(7540113804746346429L, result[^1]);
        Assert.All(result, value => Assert.True(value >= 0));
    }

    //  FunctionA 

    [Fact]
    public void FunctionA_AtDomainBoundaryOne_ReturnsExpected()
    {
        // При x = 1: sin(5) * ch(0) + e^5 = sin(5) + e^5 ≈ 147.45423482791347
        double result = MathAlgorithms.FunctionA(1);

        Assert.Equal(147.45423482791347, result, 9);
    }

    [Theory]
    [InlineData(2, 22027.38928558276)]
    [InlineData(5, 72004899340.55165)]
    public void FunctionA_InsideDomain_ReturnsExpected(double x, double expected)
    {
        // Act
        double result = MathAlgorithms.FunctionA(x);

        // Assert: сравниваем с относительной точностью 1e-9
        Assert.Equal(expected, result, Math.Abs(expected) * 1e-9);
    }

    [Theory]
    [InlineData(0)]          // деление на ноль в 5/x
    [InlineData(0.999)]      // чуть левее границы: sqrt от отрицательного
    [InlineData(-3)]
    [InlineData(double.NaN)]
    public void FunctionA_OutsideDomain_ThrowsArgumentOutOfRange(double x)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MathAlgorithms.FunctionA(x));
    }

    [Fact]
    public void FunctionA_LargestSafeX_ReturnsFiniteValue()
    {
        // e^(5*141) = e^705 ≈ 1.5e306 — ещё помещается в double
        double result = MathAlgorithms.FunctionA(141);

        Assert.True(double.IsFinite(result));
    }

    [Fact]
    public void FunctionA_ResultOverflowsDouble_ThrowsOverflow()
    {
        // e^(5*142) = e^710 > double.MaxValue (≈ e^709.78)
        Assert.Throws<OverflowException>(() => MathAlgorithms.FunctionA(142));
    }

    //  SinTaylor 

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(Math.PI / 2)]
    [InlineData(-1.2)]
    [InlineData(100)]        // большой x: проверяем приведение к [-pi; pi]
    public void SinTaylor_MatchesMathSin(double x)
    {
        // Act
        double result = MathAlgorithms.SinTaylor(x, 1e-6);

        // Assert: отличие от Math.Sin не больше 1e-5
        Assert.Equal(Math.Sin(x), result, 1e-5);
    }

    [Fact]
    public void SinTaylor_OfZero_ReturnsZero()
    {
        // Граничный случай: первый же член ряда равен 0, сумма тоже 0
        double result = MathAlgorithms.SinTaylor(0, 1e-6);

        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1e-6)]
    public void SinTaylor_NonPositiveEps_Throws(double eps)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MathAlgorithms.SinTaylor(1, eps));
    }
}
