namespace OnlineStore;

public sealed record PaymentReceipt(Money AmountPaid, Money Change);

public interface IPaymentMethod
{
    PaymentReceipt Pay(Money amount);
}

public sealed class CardPayment : IPaymentMethod
{
    public PaymentReceipt Pay(Money amount) => new(amount, new Money(0));
}

public sealed class CashPayment : IPaymentMethod
{
    public Money CashTendered { get; }

    public CashPayment(Money cashTendered) => CashTendered = cashTendered;

    public PaymentReceipt Pay(Money amount)
    {
        if (CashTendered.Amount < amount.Amount)
            throw new InvalidOperationException(
                $"Недостаточно наличных: внесено {CashTendered}, требуется {amount}.");

        return new PaymentReceipt(amount, CashTendered - amount);
    }
}
