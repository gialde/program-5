public class WarehouseManager
{
    private readonly List<Warehouse> _warehouses = new List<Warehouse>();
    private readonly List<string> _log = new List<string>();

    public IReadOnlyList<Warehouse> Warehouses => _warehouses.AsReadOnly();
    public IReadOnlyList<string> Log => _log;

    public void AddWarehouse(Warehouse warehouse)
    {
        if (warehouse != null)
            _warehouses.Add(warehouse);
    }

    public Warehouse GetWarehouse(int id) => _warehouses.FirstOrDefault(w => w.Id == id);

    private void AddLog(string message)
    {
        var logEntry = $"{DateTime.Now:HH:mm:ss} - {message}";
        _log.Add(logEntry);
        Console.WriteLine(logEntry);
    }

    public void DeliverProducts(List<Product> products, Action<string> customLog = null)
    {
        var logAction = customLog ?? AddLog;

        if (products == null || !products.Any())
        {
            logAction("Ошибка: Нечего поставлять.");
            return;
        }

        bool hasShortLife = products.Any(p => p.IsShortShelfLife);
        bool hasLongLife = products.Any(p => p.IsLongShelfLife);

        var suitableWarehouses = GetSuitableWarehousesForDelivery(hasShortLife, hasLongLife);

        if (!suitableWarehouses.Any())
        {
            logAction("Ошибка: Нет подходящих складов для поставки!");
            return;
        }

        foreach (var product in products)
        {
            var placed = false;
            foreach (var warehouse in suitableWarehouses.Where(w => w.CanAcceptProduct(product)))
            {
                if (warehouse.AddProduct(product))
                {
                    logAction($"Товар '{product.Name}' (ID: {product.Id}, объём: {product.UnitVolume:F2}) " +
                             $"размещён на складе {warehouse.Id} ({warehouse.Type.ToRussianString()})");
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                logAction($"Ошибка: Не удалось разместить товар '{product.Name}' (ID: {product.Id}) - нет подходящего склада с достаточным объёмом");
            }
        }
    }

    private List<Warehouse> GetSuitableWarehousesForDelivery(bool hasShortLife, bool hasLongLife)
    {
        if (hasShortLife && hasLongLife)
            return _warehouses.Where(w => w.Type == WarehouseType.Sorting).ToList();
        else if (hasShortLife)
            return _warehouses.Where(w => w.Type == WarehouseType.Cold).ToList();
        else
            return _warehouses.Where(w => w.Type == WarehouseType.General).ToList();
    }

    public void OptimizeSortingWarehouses(Action<string> customLog = null)
    {
        var logAction = customLog ?? AddLog;
        var sortingWarehouses = _warehouses.Where(w => w.Type == WarehouseType.Sorting).ToList();
        var generalWarehouses = _warehouses.Where(w => w.Type == WarehouseType.General).ToList();
        var coldWarehouses = _warehouses.Where(w => w.Type == WarehouseType.Cold).ToList();

        logAction("Начало оптимизации сортировочных складов...");

        foreach (var sortingWarehouse in sortingWarehouses)
        {
            var productsToMove = sortingWarehouse.Products.ToList();
            
            foreach (var product in productsToMove)
            {
                var targetWarehouses = product.IsLongShelfLife ? generalWarehouses : coldWarehouses;
                var moved = MoveProductToSuitableWarehouse(product, sortingWarehouse, targetWarehouses, logAction);
                
                if (!moved)
                {
                    logAction($"Не удалось переместить товар {product.Id} с сортировочного склада {sortingWarehouse.Id}");
                }
            }
        }
    }

    public bool MoveProduct(int productId, int fromWarehouseId, int toWarehouseId, Action<string> customLog = null)
    {
        var logAction = customLog ?? AddLog;
        var fromWarehouse = GetWarehouse(fromWarehouseId);
        var toWarehouse = GetWarehouse(toWarehouseId);

        if (fromWarehouse == null || toWarehouse == null)
        {
            logAction("Ошибка: Один из складов не найден");
            return false;
        }

        var product = fromWarehouse.GetProduct(productId);
        if (product == null)
        {
            logAction($"Ошибка: Товар с ID {productId} не найден на складе {fromWarehouseId}");
            return false;
        }

        if (!toWarehouse.CanAcceptProduct(product))
        {
            logAction($"Ошибка: На целевом складе {toWarehouseId} недостаточно места для товара {productId}");
            return false;
        }

        if (fromWarehouse.RemoveProduct(productId) && toWarehouse.AddProduct(product))
        {
            logAction($"Товар '{product.Name}' (ID: {productId}) перемещён со склада {fromWarehouseId} на склад {toWarehouseId}");
            return true;
        }

        return false;
    }

    public void MoveExpiredProducts(Action<string> customLog = null)
    {
        var logAction = customLog ?? AddLog;
        var disposalWarehouses = _warehouses.Where(w => w.Type == WarehouseType.Disposal).ToList();

        if (!disposalWarehouses.Any())
        {
            logAction("Ошибка: Нет доступных складов утилизации!");
            return;
        }

        logAction("Поиск и перемещение просроченных товаров...");

        foreach (var warehouse in _warehouses.Where(w => w.Type != WarehouseType.Disposal))
        {
            var expiredProducts = warehouse.GetExpiredProducts();
            
            foreach (var product in expiredProducts)
            {
                var moved = MoveProductToSuitableWarehouse(product, warehouse, disposalWarehouses, logAction);
                if (!moved)
                {
                    logAction($"Не удалось переместить просроченный товар {product.Id} на склад утилизации");
                }
            }
        }
    }

    private bool MoveProductToSuitableWarehouse(Product product, Warehouse fromWarehouse, 
        List<Warehouse> targetWarehouses, Action<string> logAction)
    {
        foreach (var targetWarehouse in targetWarehouses.Where(w => w.CanAcceptProduct(product)))
        {
            if (fromWarehouse.RemoveProduct(product.Id) && targetWarehouse.AddProduct(product))
            {
                logAction($"Товар '{product.Name}' (ID: {product.Id}) перемещён со склада {fromWarehouse.Id} на склад {targetWarehouse.Id}");
                return true;
            }
        }
        return false;
    }

    public void AnalyzeNetwork(Action<string> customLog = null)
    {
        var logAction = customLog ?? AddLog;
        logAction("\n=== АНАЛИЗ СКЛАДСКОЙ СЕТИ ===");

        foreach (var warehouse in _warehouses)
        {
            var status = warehouse.GetStatus();
            var issues = new List<string>();

            if (status.NeedsOptimization) issues.Add("требуется оптимизация");
            if (status.HasExpiredProducts) issues.Add("есть просроченные товары");
            if (status.IsAlmostFull) issues.Add("мало свободного места");

            var statusText = issues.Count == 0 ? "✅ Нарушений нет" : $"⚠️  Проблемы: {string.Join(", ", issues)}";
            logAction($"Склад {warehouse.Id} ({warehouse.Type.ToRussianString()}): {statusText}");
        }
    }

    public double CalculateTotalValueOnWarehouse(int warehouseId)
    {
        var warehouse = GetWarehouse(warehouseId);
        return warehouse?.TotalValue ?? 0;
    }

    public void DisplayAllWarehouses()
    {
        Console.WriteLine("\n=== СОСТОЯНИЕ ВСЕХ СКЛАДОВ ===");
        foreach (var warehouse in _warehouses)
        {
            warehouse.DisplayInfo();
        }
    }

    public void DisplayLog()
    {
        Console.WriteLine("\n=== ЖУРНАЛ СОБЫТИЙ ===");
        foreach (var logEntry in _log)
        {
            Console.WriteLine(logEntry);
        }
    }

    public void DisplayWarehouseInfo(int warehouseId)
    {
        var warehouse = GetWarehouse(warehouseId);
        if (warehouse != null)
        {
            warehouse.DisplayInfo();
        }
        else
        {
            Console.WriteLine($"Склад с ID {warehouseId} не найден!");
        }
    }
}