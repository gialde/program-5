using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    private static WarehouseManager manager = new WarehouseManager();

    static void Main()
    {
        // Создание базовых складов
        InitializeWarehouses();
        
        Console.WriteLine("=== СИСТЕМА СКЛАДСКОГО УЧЁТА ===");
        Console.WriteLine("Инициализация завершена. Создано 4 склада.\n");
        
        // Запуск интерактивного меню
        ShowInteractiveMenu();
    }

    static void InitializeWarehouses()
    {
        manager.AddWarehouse(new Warehouse(WarehouseType.General, 1000, "ул. Центральная, 1"));
        manager.AddWarehouse(new Warehouse(WarehouseType.Cold, 500, "ул. Холодильная, 2"));
        manager.AddWarehouse(new Warehouse(WarehouseType.Sorting, 800, "ул. Сортировочная, 3"));
        manager.AddWarehouse(new Warehouse(WarehouseType.Disposal, 300, "ул. Утиль, 4"));

        // Добавляем демо-товары для тестирования
        var demoProducts = new List<Product>
        {
            new Product(101, 1, "Яблоки", 0.5, 50, 45),
            new Product(102, 1, "Молоко", 1.0, 80, 5),
            new Product(103, 2, "Масло", 0.25, 120, 60),
            new Product(104, 2, "Кефир", 1.0, 75, 2),
            new Product(105, 3, "Просроченный сыр", 2.0, 200, 0)
        };

        manager.DeliverProducts(demoProducts, message => {});
        // Убираем очистку лога, так как IReadOnlyList не поддерживает Clear
    }

    static void ShowInteractiveMenu()
    {
        while (true)
        {
            Console.WriteLine("\n" + new string('=', 40));
            Console.WriteLine("          МЕНЮ УПРАВЛЕНИЯ СКЛАДАМИ");
            Console.WriteLine(new string('=', 40));
            Console.WriteLine("1. Показать все склады");
            Console.WriteLine("2. Показать конкретный склад");
            Console.WriteLine("3. Добавить новый склад");
            Console.WriteLine("4. Добавить поставку товаров");
            Console.WriteLine("5. Оптимизировать сортировочные склады");
            Console.WriteLine("6. Переместить просроченные товары");
            Console.WriteLine("7. Переместить товар вручную");
            Console.WriteLine("8. Проанализировать сеть складов");
            Console.WriteLine("9. Посчитать стоимость товаров на складе");
            Console.WriteLine("10. Показать журнал событий");
            Console.WriteLine("0. Выход из программы");
            Console.WriteLine(new string('=', 40));
            Console.Write("Выберите действие (0-10): ");

            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    manager.DisplayAllWarehouses();
                    break;
                case "2":
                    ShowWarehouseInfo();
                    break;
                case "3":
                    AddNewWarehouse();
                    break;
                case "4":
                    AddCustomDelivery();
                    break;
                case "5":
                    manager.OptimizeSortingWarehouses();
                    break;
                case "6":
                    manager.MoveExpiredProducts();
                    break;
                case "7":
                    ManualMove();
                    break;
                case "8":
                    manager.AnalyzeNetwork();
                    break;
                case "9":
                    CalculateWarehouseValue();
                    break;
                case "10":
                    manager.DisplayLog();
                    break;
                case "0":
                    Console.WriteLine("Выход из программы...");
                    return;
                default:
                    Console.WriteLine("Неверный выбор! Попробуйте снова.");
                    break;
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }

    static void ShowWarehouseInfo()
    {
        Console.Write("Введите ID склада: ");
        if (int.TryParse(Console.ReadLine(), out int warehouseId))
        {
            manager.DisplayWarehouseInfo(warehouseId);
        }
        else
        {
            Console.WriteLine("Неверный формат ID!");
        }
    }

    static void AddNewWarehouse()
    {
        Console.WriteLine("\n--- Добавление нового склада ---");
        
        Console.WriteLine("Типы складов:");
        Console.WriteLine("1 - Общий");
        Console.WriteLine("2 - Холодный");
        Console.WriteLine("3 - Сортировочный");
        Console.WriteLine("4 - Утилизация");
        Console.Write("Выберите тип склада (1-4): ");
        
        if (!int.TryParse(Console.ReadLine(), out int typeChoice) || typeChoice < 1 || typeChoice > 4)
        {
            Console.WriteLine("Неверный выбор типа!");
            return;
        }

        WarehouseType type = (WarehouseType)(typeChoice - 1);

        Console.Write("Введите объём склада: ");
        if (!double.TryParse(Console.ReadLine(), out double volume) || volume <= 0)
        {
            Console.WriteLine("Неверный объём!");
            return;
        }

        Console.Write("Введите адрес склада: ");
        string address = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(address))
        {
            Console.WriteLine("Адрес не может быть пустым!");
            return;
        }

        manager.AddWarehouse(new Warehouse(type, volume, address));
        Console.WriteLine($"Склад типа '{type.ToRussianString()}' успешно добавлен!");
    }

    static void AddCustomDelivery()
    {
        var products = new List<Product>();
        
        Console.WriteLine("\n--- Добавление поставки ---");
        Console.WriteLine("Введите товары (для завершения введите 0 в поле ID):");
        
        while (true)
        {
            Console.WriteLine("\n--- Новый товар ---");
            
            Console.Write("ID товара (0 для завершения): ");
            if (!int.TryParse(Console.ReadLine(), out int id) || id == 0) break;
            
            Console.Write("ID поставщика: ");
            if (!int.TryParse(Console.ReadLine(), out int supplierId))
            {
                Console.WriteLine("Неверный ID поставщика!");
                continue;
            }
            
            Console.Write("Название товара: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Название не может быть пустым!");
                continue;
            }
            
            Console.Write("Объём единицы товара: ");
            if (!double.TryParse(Console.ReadLine(), out double volume) || volume <= 0)
            {
                Console.WriteLine("Неверный объём!");
                continue;
            }
            
            Console.Write("Цена единицы товара: ");
            if (!double.TryParse(Console.ReadLine(), out double price) || price < 0)
            {
                Console.WriteLine("Неверная цена!");
                continue;
            }
            
            Console.Write("Дней до конца срока годности: ");
            if (!int.TryParse(Console.ReadLine(), out int days))
            {
                Console.WriteLine("Неверное количество дней!");
                continue;
            }
            
            try
            {
                products.Add(new Product(id, supplierId, name, volume, price, days));
                Console.WriteLine("Товар добавлен в поставку.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        
        if (products.Count > 0)
        {
            manager.DeliverProducts(products);
            Console.WriteLine($"Поставка из {products.Count} товаров обработана!");
        }
        else
        {
            Console.WriteLine("Поставка отменена - нет товаров.");
        }
    }

    static void ManualMove()
    {
        Console.WriteLine("\n--- Ручное перемещение товара ---");
        
        Console.Write("ID товара для перемещения: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("Неверный ID товара!");
            return;
        }
        
        Console.Write("ID склада-источника: ");
        if (!int.TryParse(Console.ReadLine(), out int fromId))
        {
            Console.WriteLine("Неверный ID склада-источника!");
            return;
        }
        
        Console.Write("ID склада-назначения: ");
        if (!int.TryParse(Console.ReadLine(), out int toId))
        {
            Console.WriteLine("Неверный ID склада-назначения!");
            return;
        }
        
        bool success = manager.MoveProduct(productId, fromId, toId);
        if (!success)
        {
            Console.WriteLine("Перемещение не удалось! Проверьте IDs и доступность места.");
        }
    }

    static void CalculateWarehouseValue()
    {
        Console.Write("Введите ID склада для подсчёта стоимости: ");
        if (int.TryParse(Console.ReadLine(), out int warehouseId))
        {
            double value = manager.CalculateTotalValueOnWarehouse(warehouseId);
            Console.WriteLine($"Общая стоимость товаров на складе {warehouseId}: {value:F2} руб.");
        }
        else
        {
            Console.WriteLine("Неверный формат ID!");
        }
    }
}