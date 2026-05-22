using Stride.Api.Models;
using Stride.Api.Services;

namespace Stride.Api.Storage;

public sealed class PurchaseRepository
{
    private readonly IJsonDataStore _store;
    private readonly IClock _clock;

    public PurchaseRepository(IJsonDataStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    public Task<IReadOnlyList<Purchase>> ListForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        _store.ReadAsync(
            doc => (IReadOnlyList<Purchase>)doc.Purchases
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList(),
            cancellationToken);

    public Task<Purchase?> FindForUserAsync(Guid userId, Guid purchaseId, CancellationToken cancellationToken) =>
        _store.ReadAsync(
            doc => doc.Purchases.FirstOrDefault(p => p.UserId == userId && p.Id == purchaseId),
            cancellationToken);

    public Task<Purchase> CreateAsync(
        Guid userId,
        string userEmail,
        string userUsername,
        List<PurchaseItem> items,
        decimal subtotal,
        decimal total,
        CancellationToken cancellationToken)
    {
        return _store.MutateAsync(doc =>
        {
            var purchase = new Purchase
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UserEmail = userEmail,
                UserUsername = userUsername,
                CreatedAt = _clock.UtcNow,
                Items = items,
                Subtotal = subtotal,
                Total = total,
            };

            doc.Purchases.Add(purchase);
            return purchase;
        }, cancellationToken);
    }
}
