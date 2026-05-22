namespace Stride.Api.Models;

public sealed class PurchaseItem
{
    public string ProductId { get; init; } = "";
    public string Name { get; init; } = "";
    public string? Size { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

