public class Warehouse
{
    public int Id { get; }
    public WarehouseType Type { get; set; }
    public double Volume { get; set; }
    public string Address { get; set; }

    private readonly List<Product> _products = new List<Product>();
    private static int _nextId = 1;

    public IReadOnlyList<Product> Products => _products.AsReadOnly();
    public double UsedVolume => _products.Sum(p => p.UnitVolume);
    public double FreeVolume => Volume - UsedVolume;
    public bool IsFull => FreeVolume <= 0.01;
    public double TotalValue => _products.Sum(p => p.UnitPrice);

    public Warehouse(WarehouseType type, double volume, string address, int? id = null)
    {
        Id = id ?? _nextId++;
        Type = type;
        Volume = volume > 0 ? volume : throw new ArgumentException("Объём склада должен быть > 0");
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }

    public bool CanAcceptProduct(Product product) => FreeVolume >= product?.UnitVolume;

    public bool AddProduct(Product product)
    {
        if (product == null || !CanAcceptProduct(product)) 
            return false;

        _products.Add(product);
        return true;
    }

    public bool RemoveProduct(int productId)
    {
        var product = _products.FirstOrDefault(p => p.Id == productId);
        return product != null && _products.Remove(product);
    }

    public Product GetProduct(int productId) => _products.FirstOrDefault(p => p.Id == productId);

    public List<Product> GetExpiredProducts() => _products.Where(p => p.IsExpired).ToList();
    public List<Product> GetShortShelfLifeProducts() => _products.Where(p => p.IsShortShelfLife).ToList();
    public List<Product> GetLongShelfLifeProducts() => _products.Where(p => p.IsLongShelfLife).ToList();

    public void Clear() => _products.Clear();

    public WarehouseStatus GetStatus()
    {
        var status = new WarehouseStatus { WarehouseId = Id, WarehouseType = Type };

        if (Type == WarehouseType.Sorting && _products.Count > 0)
            status.NeedsOptimization = true;

        if (GetExpiredProducts().Count > 0)
            status.HasExpiredProducts = true;

        if (FreeVolume < Volume * 0.1) // Меньше 10% свободного места
            status.IsAlmostFull = true;

        return status;
    }

    public void DisplayInfo()
    {
        Console.WriteLine($"\n=== Склад {Id} ({Type.ToRussianString()}) ===");
        Console.WriteLine($"Адрес: {Address}");
        Console.WriteLine($"Объём: {Volume:F2} (занято: {UsedVolume:F2}, свободно: {FreeVolume:F2})");
        Console.WriteLine($"Общая стоимость товаров: {TotalValue:F2}");
        Console.WriteLine($"Количество товаров: {_products.Count}");

        if (_products.Count == 0)
        {
            Console.WriteLine("  → Склад пуст");
        }
        else
        {
            Console.WriteLine("  Товары на складе:");
            foreach (var product in _products)
            {
                var status = product.IsExpired ? " [ПРОСРОЧЕН]" : 
                           product.IsShortShelfLife ? " [КОРОТКИЙ СРОК]" : "";
                Console.WriteLine($"    → {product}{status}");
            }
        }
    }
}

public class WarehouseStatus
{
    public int WarehouseId { get; set; }
    public WarehouseType WarehouseType { get; set; }
    public bool NeedsOptimization { get; set; }
    public bool HasExpiredProducts { get; set; }
    public bool IsAlmostFull { get; set; }
}