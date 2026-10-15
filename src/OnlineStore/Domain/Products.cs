namespace OnlineStore;

public abstract class Product
{
    public string Name { get; }
    public abstract Money UnitPrice { get; }
    public abstract bool RequiresDelivery { get; }
    public abstract decimal WeightKg { get; }

    protected Product(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название товара не должно быть пустым.", nameof(name));

        Name = name.Trim();
    }
}

public sealed class PhysicalProduct : Product
{
    private readonly Money _price;
    public override Money UnitPrice => _price;
    public override bool RequiresDelivery => true;
    public override decimal WeightKg { get; }

    public PhysicalProduct(string name, Money price, decimal weightKg) : base(name)
    {
        if (weightKg <= 0)
            throw new ArgumentOutOfRangeException(nameof(weightKg), "Вес должен быть больше нуля.");

        _price = price;
        WeightKg = weightKg;
    }
}

public sealed class DigitalProduct : Product
{
    private readonly Money _price;
    public override Money UnitPrice => _price;
    public decimal FileSizeMb { get; }
    public override bool RequiresDelivery => false;
    public override decimal WeightKg => 0;

    public DigitalProduct(string name, Money price, decimal fileSizeMb) : base(name)
    {
        if (fileSizeMb <= 0)
            throw new ArgumentOutOfRangeException(nameof(fileSizeMb), "Размер файла должен быть больше нуля.");

        _price = price;
        FileSizeMb = fileSizeMb;
    }
}

public sealed class SubscriptionProduct : Product
{
    public Money MonthlyPrice { get; }
    public int Months { get; }
    public override Money UnitPrice => (MonthlyPrice * Months) * (Months >= 12 ? 0.9m : 1m);
    public override bool RequiresDelivery => false;
    public override decimal WeightKg => 0;

    public SubscriptionProduct(string name, Money monthlyPrice, int months) : base(name)
    {
        if (months < 1)
            throw new ArgumentOutOfRangeException(nameof(months), "Срок подписки должен быть не меньше месяца.");

        MonthlyPrice = monthlyPrice;
        Months = months;
    }
}
