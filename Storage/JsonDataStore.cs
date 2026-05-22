using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace Stride.Api.Storage;

public sealed class JsonDataStore : IJsonDataStore
{
    private readonly string _path;
    private readonly SemaphoreSlim _mutex = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public JsonDataStore(IHostEnvironment env)
    {
        _path = Path.Combine(env.ContentRootPath, "App_Data", "stride-db.json");
    }

    public async Task<T> ReadAsync<T>(Func<DatabaseDocument, T> read, CancellationToken cancellationToken)
    {
        await _mutex.WaitAsync(cancellationToken);
        try
        {
            var document = await LoadUnsafeAsync(cancellationToken);
            return read(document);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<T> MutateAsync<T>(Func<DatabaseDocument, T> mutate, CancellationToken cancellationToken)
    {
        await _mutex.WaitAsync(cancellationToken);
        try
        {
            var document = await LoadUnsafeAsync(cancellationToken);
            var result = mutate(document);
            await SaveUnsafeAsync(document, cancellationToken);
            return result;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private async Task<DatabaseDocument> LoadUnsafeAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_path))
        {
            return new DatabaseDocument();
        }

        await using var stream = File.OpenRead(_path);
        var document = await JsonSerializer.DeserializeAsync<DatabaseDocument>(stream, JsonOptions, cancellationToken);
        return document ?? new DatabaseDocument();
    }

    private async Task SaveUnsafeAsync(DatabaseDocument document, CancellationToken cancellationToken)
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var tmp = _path + ".tmp";
        await using (var stream = File.Create(tmp))
        {
            await JsonSerializer.SerializeAsync(stream, document, JsonOptions, cancellationToken);
        }

        File.Move(tmp, _path, overwrite: true);
    }
}
