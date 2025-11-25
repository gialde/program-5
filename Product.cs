public class Product
{
    public int Id { get; }
    public int SupplierId { get; set; }
    public string Name { get; set; }
    public double UnitVolume { get; set; }
    public double UnitPrice { get; set; }
    public int DaysToExpiry { get; set; }

    public Product(int id, int supplierId, string name, double unitVolume, double unitPrice, int daysToExpiry)
    {
        Id = id;
        SupplierId = supplierId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        UnitVolume = unitVolume > 0 ? unitVolume : throw new ArgumentException("Объём должен быть > 0");
        UnitPrice = unitPrice >= 0 ? unitPrice : throw new ArgumentException("Цена не может быть отрицательной");
        DaysToExpiry = daysToExpiry;
    }

    public bool IsExpired => DaysToExpiry <= 0;
    public bool IsShortShelfLife => DaysToExpiry > 0 && DaysToExpiry < 30;
    public bool IsLongShelfLife => DaysToExpiry >= 30;

    public override string ToString() => 
        $"ID: {Id}, {Name}, объём: {UnitVolume:F2}, цена: {UnitPrice:F2}, дней до конца: {DaysToExpiry}";

    public Product Clone() => new Product(Id, SupplierId, Name, UnitVolume, UnitPrice, DaysToExpiry);
}