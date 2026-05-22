namespace Stride.Api.Models;

public sealed class Purchase
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserEmail { get; init; } = "";
    public string UserUsername { get; init; } = "";
    public DateTimeOffset CreatedAt { get; init; }

    public decimal Subtotal { get; init; }
    public decimal Total { get; init; }

    public List<PurchaseItem> Items { get; init; } = new();
}
