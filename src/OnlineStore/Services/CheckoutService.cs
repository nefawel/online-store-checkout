namespace OnlineStore;

public sealed record CheckoutSummary(
    Money Subtotal,
    Money Discount,
    Money DeliveryCost,
    Money Total,
    string DeliveryName);

public sealed class CheckoutService
{
    public CheckoutSummary Calculate(Order order, DiscountPolicy discountPolicy, DeliveryMethod deliveryMethod)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(discountPolicy);
        ArgumentNullException.ThrowIfNull(deliveryMethod);

        if (order.Items.Count == 0)
            throw new InvalidOperationException("Нельзя оформить пустой заказ.");

        if (!deliveryMethod.IsAvailableFor(order))
            throw new InvalidOperationException(
                $"Способ доставки «{deliveryMethod.Name}» не подходит для этого заказа.");

        Money subtotal = order.Subtotal;
        Money discount = discountPolicy.CalculateDiscount(order);
        if (discount.Amount > subtotal.Amount)
            throw new InvalidOperationException("Скидка не может превышать стоимость товаров.");

        Money deliveryCost = deliveryMethod.CalculateCost(order);
        Money total = subtotal - discount + deliveryCost;
        return new CheckoutSummary(subtotal, discount, deliveryCost, total, deliveryMethod.Name);
    }

    public PaymentReceipt Pay(
        Order order,
        DiscountPolicy discountPolicy,
        DeliveryMethod deliveryMethod,
        IPaymentMethod paymentMethod)
    {
        ArgumentNullException.ThrowIfNull(paymentMethod);
        CheckoutSummary summary = Calculate(order, discountPolicy, deliveryMethod);
        return paymentMethod.Pay(summary.Total);
    }
}
