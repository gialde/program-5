public enum WarehouseType
{
    General,
    Cold,
    Sorting,
    Disposal
}

public static class WarehouseTypeExtensions
{
    public static string ToRussianString(this WarehouseType type)
    {
        return type switch
        {
            WarehouseType.General => "общий",
            WarehouseType.Cold => "холодный",
            WarehouseType.Sorting => "сортировочный",
            WarehouseType.Disposal => "утилизация",
            _ => "неизвестный"
        };
    }
}