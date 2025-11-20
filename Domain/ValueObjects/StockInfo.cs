namespace Domain.ValueObjects;

public sealed record StockInfo
{
    public long TotalStock { get; init; }
    public long BookedStock { get; init; }
    public long AvailableStock => TotalStock - BookedStock;
    public bool IsInStock => AvailableStock > 0;
    public string StockStatus => IsInStock ? "in_stock" : "out_of_stock";

    public StockInfo(long totalStock, long bookedStock)
    {
        if (totalStock < 0)
        {
            throw new ArgumentException("Total stock cannot be negative.", nameof(totalStock));
        }

        if (bookedStock < 0)
        {
            throw new ArgumentException("Booked stock cannot be negative.", nameof(bookedStock));
        }

        TotalStock = totalStock;
        BookedStock = bookedStock;
    }

    public bool CanFulfillOrder(long quantity)
    {
        return AvailableStock >= quantity && quantity > 0;
    }
}
