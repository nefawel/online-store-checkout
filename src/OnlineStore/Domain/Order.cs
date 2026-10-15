using System.Collections.ObjectModel;

namespace OnlineStore;

public sealed class OrderItem
{
    public Product Product { get; }
    public int Quantity { get; }
    public Money Total => Product.UnitPrice * Quantity;

    public OrderItem(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        if (quantity < 1)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Количество товара должно быть не меньше 1.");

        Product = product;
        Quantity = quantity;
    }
}

public sealed class Order
{
    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items { get; }

    public Order() => Items = new ReadOnlyCollection<OrderItem>(_items);

    public void AddProduct(Product product) => AddProduct(product, 1);
    public void AddProduct(Product product, int quantity) => _items.Add(new OrderItem(product, quantity));

    public Money Subtotal => _items.Aggregate(new Money(0), (sum, item) => sum + item.Total);
    public bool HasPhysicalProducts => _items.Any(item => item.Product.RequiresDelivery);
    public decimal TotalWeightKg => _items
        .Where(item => item.Product.RequiresDelivery)
        .Sum(item => item.Product.WeightKg * item.Quantity);
}
