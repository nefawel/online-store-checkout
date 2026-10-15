namespace OnlineStore;

public abstract class DiscountPolicy
{
    public abstract Money CalculateDiscount(Order order);
}

public sealed class NoDiscount : DiscountPolicy
{
    public override Money CalculateDiscount(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return new Money(0);
    }
}

public sealed class PercentageDiscount : DiscountPolicy
{
    public decimal Percentage { get; }

    public PercentageDiscount(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage), "Процент скидки должен быть от 0 до 100.");

        Percentage = percentage;
    }

    public override Money CalculateDiscount(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return order.Subtotal * (Percentage / 100m);
    }
}

public sealed class FixedDiscount : DiscountPolicy
{
    public Money Amount { get; }

    public FixedDiscount(Money amount) => Amount = amount;

    public override Money CalculateDiscount(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (Amount.Amount > order.Subtotal.Amount)
            throw new InvalidOperationException("Фиксированная скидка не может превышать стоимость товаров.");

        return Amount;
    }
}
