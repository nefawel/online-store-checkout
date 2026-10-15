using System.Text;
using OnlineStore;

namespace OnlineStore.Tests;

internal static class Program
{
    private static int _passed;
    private static int _failed;

    private static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        MoneyTests();
        ProductTests();
        OrderTests();
        DiscountTests();
        DeliveryTests();
        PaymentTests();
        CheckoutTests();

        Console.WriteLine($"\nРезультат: {_passed} успешно, {_failed} ошибок, всего {_passed + _failed}.");
        return _failed == 0 ? 0 : 1;
    }

    private static void MoneyTests()
    {
        Run("Money: ноль и decimal без погрешностей", () =>
        {
            Equal(0m, new Money(0m).Amount);
            Equal(0.3m, (new Money(0.1m) + new Money(0.2m)).Amount);
        });
        Run("Money: округление до копеек, середина от нуля", () =>
        {
            Equal(1.24m, new Money(1.235m).Amount);
            Equal(1.23m, new Money(1.234m).Amount);
            Equal(0.01m, new Money(0.005m).Amount);
        });
        Run("Money: отрицательная сумма запрещена до округления", () =>
        {
            Throws<ArgumentOutOfRangeException>(() => new Money(-1m));
            Throws<ArgumentOutOfRangeException>(() => new Money(-0.001m));
        });
        Run("Money: сложение, вычитание и нулевой остаток", () =>
        {
            Equal(300.30m, (new Money(100.10m) + new Money(200.20m)).Amount);
            Equal(75m, (new Money(100m) - new Money(25m)).Amount);
            Equal(0m, (new Money(100m) - new Money(100m)).Amount);
        });
        Run("Money: вычитание с отрицательным результатом запрещено", () =>
            Throws<InvalidOperationException>(() => _ = new Money(10m) - new Money(11m)));
        Run("Money: целый и дробный множители", () =>
        {
            Equal(37.5m, (new Money(12.5m) * 3).Amount);
            Equal(15.63m, (new Money(12.5m) * 1.25m).Amount);
            Equal(0m, (new Money(12.5m) * 0).Amount);
        });
        Run("Money: отрицательные множители запрещены", () =>
        {
            Throws<ArgumentOutOfRangeException>(() => _ = new Money(10m) * -1);
            Throws<ArgumentOutOfRangeException>(() => _ = new Money(10m) * -0.5m);
        });
        Run("Money: формат рублей с разделителем тысяч", () =>
            Equal("1 250,00 ₽", new Money(1250m).ToString()));
    }

    private static void ProductTests()
    {
        Run("Товары: физический товар раскрывает цену, вес и доставку", () =>
        {
            Product product = new PhysicalProduct("Книга", new Money(700m), 0.75m);
            Equal("Книга", product.Name);
            Equal(700m, product.UnitPrice.Amount);
            Equal(0.75m, product.WeightKg);
            True(product.RequiresDelivery);
        });
        Run("Товары: цифровому товару не нужны вес и доставка", () =>
        {
            Product product = new DigitalProduct("Курс", new Money(500m), 120m);
            Equal(500m, product.UnitPrice.Amount);
            Equal(0m, product.WeightKg);
            False(product.RequiresDelivery);
        });
        Run("Подписка: 11 месяцев без скидки", () =>
            Equal(1100m, new SubscriptionProduct("Доступ", new Money(100m), 11).UnitPrice.Amount));
        Run("Подписка: скидка 10% начинается с 12 месяцев", () =>
        {
            var product = new SubscriptionProduct("Доступ", new Money(100m), 12);
            Equal(1080m, product.UnitPrice.Amount);
            Equal(100m, product.MonthlyPrice.Amount);
            Equal(12, product.Months);
            Equal(0m, product.WeightKg);
            False(product.RequiresDelivery);
        });
        Run("Подписка: скидка сохраняется на срок больше года", () =>
            Equal(2160m, new SubscriptionProduct("Доступ", new Money(100m), 24).UnitPrice.Amount));
        Run("Товары: пустые названия запрещены", () =>
        {
            Throws<ArgumentException>(() => new PhysicalProduct(" ", new Money(1m), 1m));
            Throws<ArgumentException>(() => new DigitalProduct("", new Money(1m), 1m));
            Throws<ArgumentException>(() => new SubscriptionProduct("\t", new Money(1m), 1));
        });
        Run("Товары: отрицательные вес и размер файла запрещены", () =>
        {
            Throws<ArgumentOutOfRangeException>(() => new PhysicalProduct("Товар", new Money(1m), -1m));
            Throws<ArgumentOutOfRangeException>(() => new DigitalProduct("Файл", new Money(1m), -1m));
        });
        Run("Подписка: нулевой и отрицательный срок запрещены", () =>
        {
            Throws<ArgumentOutOfRangeException>(() => new SubscriptionProduct("Доступ", new Money(1m), 0));
            Throws<ArgumentOutOfRangeException>(() => new SubscriptionProduct("Доступ", new Money(1m), -1));
        });
    }

    private static void OrderTests()
    {
        Run("Заказ: исходное состояние пустое", () =>
        {
            var order = new Order();
            Equal(0, order.Items.Count);
            Equal(0m, order.Subtotal.Amount);
            Equal(0m, order.TotalWeightKg);
            False(order.HasPhysicalProducts);
        });
        Run("Заказ: обе перегрузки добавления учитывают количество", () =>
        {
            var order = new Order();
            order.AddProduct(new DigitalProduct("A", new Money(25m), 1m));
            order.AddProduct(new DigitalProduct("B", new Money(30m), 1m), 3);
            Equal(2, order.Items.Count);
            Equal(1, order.Items[0].Quantity);
            Equal(3, order.Items[1].Quantity);
            Equal(115m, order.Subtotal.Amount);
        });
        Run("Позиция: сохраняет товар и умножает его цену", () =>
        {
            var product = new SubscriptionProduct("Подписка", new Money(100m), 12);
            var item = new OrderItem(product, 2);
            True(ReferenceEquals(product, item.Product));
            Equal(2, item.Quantity);
            Equal(2160m, item.Total.Amount);
        });
        Run("Заказ: вес умножается на количество физических товаров", () =>
        {
            var order = MixedOrder();
            Equal(2.5m, order.TotalWeightKg);
            True(order.HasPhysicalProducts);
            Equal(2500m, order.Subtotal.Amount);
        });
        Run("Заказ: нулевое и отрицательное количество запрещены", () =>
        {
            var order = new Order();
            var product = new DigitalProduct("Файл", new Money(1m), 1m);
            Throws<ArgumentOutOfRangeException>(() => order.AddProduct(product, 0));
            Throws<ArgumentOutOfRangeException>(() => order.AddProduct(product, -1));
            Throws<ArgumentOutOfRangeException>(() => new OrderItem(product, 0));
            Equal(0, order.Items.Count);
        });
        Run("Заказ: null вместо товара запрещён", () =>
        {
            Throws<ArgumentNullException>(() => new Order().AddProduct(null!));
            Throws<ArgumentNullException>(() => new OrderItem(null!, 1));
        });
    }

    private static void DiscountTests()
    {
        Run("Скидки: отсутствие скидки", () =>
            Equal(0m, new NoDiscount().CalculateDiscount(DigitalOrder(100m)).Amount));
        Run("Скидки: процент рассчитывается от стоимости товаров", () =>
            Equal(250m, new PercentageDiscount(10m).CalculateDiscount(MixedOrder()).Amount));
        Run("Скидки: границы 0% и 100%", () =>
        {
            Equal(0m, new PercentageDiscount(0m).CalculateDiscount(DigitalOrder(100m)).Amount);
            Equal(100m, new PercentageDiscount(100m).CalculateDiscount(DigitalOrder(100m)).Amount);
        });
        Run("Скидки: процент вне диапазона запрещён", () =>
        {
            Throws<ArgumentOutOfRangeException>(() => new PercentageDiscount(-0.01m));
            Throws<ArgumentOutOfRangeException>(() => new PercentageDiscount(100.01m));
        });
        Run("Скидки: фиксированная сумма и полная скидка", () =>
        {
            Equal(20m, new FixedDiscount(new Money(20m)).CalculateDiscount(DigitalOrder(100m)).Amount);
            Equal(100m, new FixedDiscount(new Money(100m)).CalculateDiscount(DigitalOrder(100m)).Amount);
        });
        Run("Скидки: фиксированная скидка выше стоимости запрещена", () =>
            Throws<InvalidOperationException>(() => new FixedDiscount(new Money(101m)).CalculateDiscount(DigitalOrder(100m))));
    }

    private static void DeliveryTests()
    {
        Run("Доставка: курьер учитывает дробный вес и количество", () =>
            Equal(500m, new CourierDelivery().CalculateCost(MixedOrder()).Amount));
        Run("Доставка: самовывоз бесплатный для смешанного заказа", () =>
        {
            var delivery = new PickupDelivery();
            True(delivery.IsAvailableFor(MixedOrder()));
            Equal(0m, delivery.CalculateCost(MixedOrder()).Amount);
        });
        Run("Доставка: цифровая доставка бесплатна без физических товаров", () =>
        {
            var delivery = new DigitalDelivery();
            True(delivery.IsAvailableFor(DigitalOrder(100m)));
            Equal(0m, delivery.CalculateCost(DigitalOrder(100m)).Amount);
        });
        Run("Доставка: курьер и самовывоз недоступны цифровому заказу", () =>
        {
            False(new CourierDelivery().IsAvailableFor(DigitalOrder(100m)));
            False(new PickupDelivery().IsAvailableFor(DigitalOrder(100m)));
        });
        Run("Доставка: цифровая доставка недоступна смешанному заказу", () =>
            False(new DigitalDelivery().IsAvailableFor(MixedOrder())));
        Run("Доставка: прямой расчёт неподходящего способа запрещён", () =>
        {
            Throws<InvalidOperationException>(() => new CourierDelivery().CalculateCost(DigitalOrder(100m)));
            Throws<InvalidOperationException>(() => new PickupDelivery().CalculateCost(DigitalOrder(100m)));
            Throws<InvalidOperationException>(() => new DigitalDelivery().CalculateCost(MixedOrder()));
        });
    }

    private static void PaymentTests()
    {
        Run("Оплата: карта возвращает оплаченную сумму без сдачи", () =>
        {
            IPaymentMethod payment = new CardPayment();
            var receipt = payment.Pay(new Money(123.45m));
            Equal(123.45m, receipt.AmountPaid.Amount);
            Equal(0m, receipt.Change.Amount);
        });
        Run("Оплата: наличные со сдачей", () =>
        {
            IPaymentMethod payment = new CashPayment(new Money(200m));
            var receipt = payment.Pay(new Money(123.45m));
            Equal(123.45m, receipt.AmountPaid.Amount);
            Equal(76.55m, receipt.Change.Amount);
        });
        Run("Оплата: наличные без сдачи", () =>
        {
            var receipt = new CashPayment(new Money(100m)).Pay(new Money(100m));
            Equal(100m, receipt.AmountPaid.Amount);
            Equal(0m, receipt.Change.Amount);
        });
        Run("Оплата: нехватка одной копейки отклоняется", () =>
            Throws<InvalidOperationException>(() => new CashPayment(new Money(99.99m)).Pay(new Money(100m))));
    }

    private static void CheckoutTests()
    {
        Run("Оформление: товары минус скидка плюс доставка", () =>
        {
            var summary = new CheckoutService().Calculate(MixedOrder(), new PercentageDiscount(10m), new CourierDelivery());
            Equal(2500m, summary.Subtotal.Amount);
            Equal(250m, summary.Discount.Amount);
            Equal(500m, summary.DeliveryCost.Amount);
            Equal(2750m, summary.Total.Amount);
            Equal(new CourierDelivery().Name, summary.DeliveryName);
        });
        Run("Оформление: подписка со своей скидкой и скидкой заказа", () =>
        {
            var order = new Order();
            order.AddProduct(new SubscriptionProduct("Подписка", new Money(100m), 12));
            var summary = new CheckoutService().Calculate(order, new PercentageDiscount(10m), new DigitalDelivery());
            Equal(1080m, summary.Subtotal.Amount);
            Equal(972m, summary.Total.Amount);
        });
        Run("Оформление: скидка 100% не отменяет стоимость доставки", () =>
        {
            var summary = new CheckoutService().Calculate(MixedOrder(), new PercentageDiscount(100m), new CourierDelivery());
            Equal(500m, summary.Total.Amount);
        });
        Run("Оформление: пустой заказ отклоняется", () =>
            Throws<InvalidOperationException>(() => new CheckoutService().Calculate(new Order(), new NoDiscount(), new DigitalDelivery())));
        Run("Оформление: неподходящий способ доставки отклоняется", () =>
        {
            Throws<InvalidOperationException>(() => new CheckoutService().Calculate(MixedOrder(), new NoDiscount(), new DigitalDelivery()));
            Throws<InvalidOperationException>(() => new CheckoutService().Calculate(DigitalOrder(100m), new NoDiscount(), new CourierDelivery()));
        });
        Run("Оформление: собственные политики работают полиморфно", () =>
        {
            var summary = new CheckoutService().Calculate(DigitalOrder(100m), new CustomDiscount(17m), new CustomDelivery(true, 23m));
            Equal(17m, summary.Discount.Amount);
            Equal(23m, summary.DeliveryCost.Amount);
            Equal(106m, summary.Total.Amount);
            Equal("Тестовая доставка", summary.DeliveryName);
        });
        Run("Оформление: чрезмерная скидка сторонней политики отклоняется", () =>
            Throws<InvalidOperationException>(() => new CheckoutService().Calculate(DigitalOrder(100m), new CustomDiscount(101m), new DigitalDelivery())));
        Run("Оформление: доступность сторонней доставки проверяется", () =>
            Throws<InvalidOperationException>(() => new CheckoutService().Calculate(DigitalOrder(100m), new NoDiscount(), new CustomDelivery(false, 0m))));
        Run("Оформление: Pay передаёт итог методу оплаты через интерфейс", () =>
        {
            var payment = new RecordingPayment();
            var receipt = new CheckoutService().Pay(MixedOrder(), new PercentageDiscount(10m), new CourierDelivery(), payment);
            Equal(1, payment.Calls);
            Equal(2750m, payment.LastAmount.Amount);
            Equal(2750m, receipt.AmountPaid.Amount);
        });
        Run("Оформление: ошибка расчёта не вызывает оплату", () =>
        {
            var payment = new RecordingPayment();
            Throws<InvalidOperationException>(() => new CheckoutService().Pay(new Order(), new NoDiscount(), new DigitalDelivery(), payment));
            Equal(0, payment.Calls);
        });
        Run("Оформление: null в обязательных аргументах отклоняется", () =>
        {
            var service = new CheckoutService();
            var order = DigitalOrder(100m);
            Throws<ArgumentNullException>(() => service.Calculate(null!, new NoDiscount(), new DigitalDelivery()));
            Throws<ArgumentNullException>(() => service.Calculate(order, null!, new DigitalDelivery()));
            Throws<ArgumentNullException>(() => service.Calculate(order, new NoDiscount(), null!));
            Throws<ArgumentNullException>(() => service.Pay(order, new NoDiscount(), new DigitalDelivery(), null!));
        });
    }

    private static Order DigitalOrder(decimal price)
    {
        var order = new Order();
        order.AddProduct(new DigitalProduct("Файл", new Money(price), 10m));
        return order;
    }

    private static Order MixedOrder()
    {
        var order = new Order();
        order.AddProduct(new PhysicalProduct("Книга", new Money(1000m), 1.25m), 2);
        order.AddProduct(new DigitalProduct("Курс", new Money(500m), 100m));
        return order;
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            _passed++;
            Console.WriteLine($"[OK] {name}");
        }
        catch (Exception exception)
        {
            _failed++;
            Console.WriteLine($"[FAIL] {name}\n       {exception.GetType().Name}: {exception.Message}");
        }
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"Ожидалось: {expected}; получено: {actual}.");
    }

    private static void True(bool condition)
    {
        if (!condition)
            throw new InvalidOperationException("Ожидалось true, получено false.");
    }

    private static void False(bool condition) => True(!condition);

    private static void Throws<TException>(Action action) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException($"Ожидалось исключение {typeof(TException).Name}.");
    }

    private sealed class CustomDiscount(decimal amount) : DiscountPolicy
    {
        public override Money CalculateDiscount(Order order) => new(amount);
    }

    private sealed class CustomDelivery(bool available, decimal cost) : DeliveryMethod
    {
        public override string Name => "Тестовая доставка";
        public override bool IsAvailableFor(Order order) => available;
        public override Money CalculateCost(Order order) => new(cost);
    }

    private sealed class RecordingPayment : IPaymentMethod
    {
        public int Calls { get; private set; }
        public Money LastAmount { get; private set; }

        public PaymentReceipt Pay(Money amount)
        {
            Calls++;
            LastAmount = amount;
            return new CardPayment().Pay(amount);
        }
    }
}
