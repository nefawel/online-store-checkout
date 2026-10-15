using System.Globalization;

namespace OnlineStore;

/// <summary>Неотрицательная денежная сумма в рублях с точностью до копейки.</summary>
public readonly record struct Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Денежная сумма не может быть отрицательной.");

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

    public static Money operator -(Money left, Money right)
    {
        if (right.Amount > left.Amount)
            throw new InvalidOperationException("Результат вычитания денежных сумм не может быть отрицательным.");

        return new Money(left.Amount - right.Amount);
    }

    public static Money operator *(Money money, int multiplier) => money * (decimal)multiplier;

    public static Money operator *(Money money, decimal multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Множитель не может быть отрицательным.");

        return new Money(money.Amount * multiplier);
    }

    public static Money operator *(int multiplier, Money money) => money * multiplier;
    public static Money operator *(decimal multiplier, Money money) => money * multiplier;

    public override string ToString() => Amount
        .ToString("N2", CultureInfo.GetCultureInfo("ru-RU"))
        .Replace('\u00a0', ' ')
        .Replace('\u202f', ' ') + " ₽";
}
