namespace OnlineStore;

public abstract class DeliveryMethod
{
    public abstract string Name { get; }
    public abstract bool IsAvailableFor(Order order);
    public abstract Money CalculateCost(Order order);

    protected void EnsureAvailable(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (!IsAvailableFor(order))
            throw new InvalidOperationException($"Способ доставки «{Name}» не подходит для этого заказа.");
    }
}

public sealed class CourierDelivery : DeliveryMethod
{
    public override string Name => "Курьерская доставка";

    public override bool IsAvailableFor(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return order.HasPhysicalProducts;
    }

    public override Money CalculateCost(Order order)
    {
        EnsureAvailable(order);
        return new Money(300) + new Money(80) * order.TotalWeightKg;
    }
}

public sealed class PickupDelivery : DeliveryMethod
{
    public override string Name => "Самовывоз";

    public override bool IsAvailableFor(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return order.HasPhysicalProducts;
    }

    public override Money CalculateCost(Order order)
    {
        EnsureAvailable(order);
        return new Money(0);
    }
}

public sealed class DigitalDelivery : DeliveryMethod
{
    public override string Name => "Цифровая доставка";

    public override bool IsAvailableFor(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return !order.HasPhysicalProducts;
    }

    public override Money CalculateCost(Order order)
    {
        EnsureAvailable(order);
        return new Money(0);
    }
}
