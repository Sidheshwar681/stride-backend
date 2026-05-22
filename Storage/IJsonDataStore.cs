namespace Stride.Api.Storage;

public interface IJsonDataStore
{
    Task<T> ReadAsync<T>(Func<DatabaseDocument, T> read, CancellationToken cancellationToken);
    Task<T> MutateAsync<T>(Func<DatabaseDocument, T> mutate, CancellationToken cancellationToken);
}
