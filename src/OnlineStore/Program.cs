using System.Globalization;
using System.Text;

namespace OnlineStore;

internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

        var keyboard = new PhysicalProduct("Механическая клавиатура", new Money(5_000), 1.2m);
        var course = new DigitalProduct("Курс C#", new Money(1_500), 750);
        var subscription = new SubscriptionProduct("Облачное хранилище", new Money(500), 12);

        var order = new Order();
        order.AddProduct(keyboard, 2);
        order.AddProduct(course);
        order.AddProduct(subscription);

        Console.WriteLine("ОФОРМЛЕНИЕ ЗАКАЗА В ИНТЕРНЕТ-МАГАЗИНЕ");
        Console.WriteLine();
        foreach (OrderItem item in order.Items)
            Console.WriteLine($"{item.Product.Name}: {item.Product.UnitPrice} × {item.Quantity} = {item.Total}");

        Console.WriteLine("Подписка на 12 месяцев уже включает скидку 10% от месячной стоимости × 12.");
        Console.WriteLine($"Общий вес физических товаров: {order.TotalWeightKg} кг");

        var checkout = new CheckoutService();
        DiscountPolicy discount = new PercentageDiscount(10);
        DeliveryMethod delivery = new CourierDelivery();
        CheckoutSummary summary = checkout.Calculate(order, discount, delivery);

        Console.WriteLine();
        Console.WriteLine($"Стоимость товаров: {summary.Subtotal}");
        Console.WriteLine($"Скидка на заказ (10%): {summary.Discount}");
        Console.WriteLine($"{summary.DeliveryName}: {summary.DeliveryCost}");
        Console.WriteLine($"ИТОГО: {summary.Total}");

        IPaymentMethod payment = new CardPayment();
        PaymentReceipt cardReceipt = checkout.Pay(order, discount, delivery, payment);
        Console.WriteLine($"Оплата картой выполнена: {cardReceipt.AmountPaid}");

        // Независимый пример альтернативного способа оплаты той же суммы.
        payment = new CashPayment(new Money(20_000));
        PaymentReceipt cashReceipt = payment.Pay(summary.Total);
        Console.WriteLine($"Пример оплаты наличными: внесено 20 000,00 ₽, сдача {cashReceipt.Change}");
        Console.WriteLine("Оплата имитируется: реальные деньги не списываются.");

        var digitalOrder = new Order();
        digitalOrder.AddProduct(course);
        CheckoutSummary digitalSummary = checkout.Calculate(digitalOrder, new NoDiscount(), new DigitalDelivery());
        CheckoutSummary pickupSummary = checkout.Calculate(order, new FixedDiscount(new Money(500)), new PickupDelivery());
        Console.WriteLine();
        Console.WriteLine($"Цифровой заказ без скидки: {digitalSummary.Total}, доставка {digitalSummary.DeliveryCost}");
        Console.WriteLine($"Самовывоз со скидкой 500,00 ₽: {pickupSummary.Total}, доставка {pickupSummary.DeliveryCost}");

        Console.WriteLine();
        Console.WriteLine("ПРОВЕРКА ОБРАБОТКИ ОШИБОК");
        ShowExpectedError("Некорректное количество", () => order.AddProduct(keyboard, 0));
        ShowExpectedError("Пустой заказ", () => checkout.Calculate(new Order(), new NoDiscount(), new DigitalDelivery()));
        ShowExpectedError("Неподходящая доставка", () => checkout.Calculate(order, discount, new DigitalDelivery()));
        ShowExpectedError("Недостаточно наличных", () => checkout.Pay(order, discount, delivery, new CashPayment(new Money(1_000))));
    }

    private static void ShowExpectedError(string scenario, Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            Console.WriteLine($"{scenario}: {exception.Message}");
        }
    }
}
