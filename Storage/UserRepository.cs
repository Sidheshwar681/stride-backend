using Stride.Api.Models;
using Stride.Api.Services;

namespace Stride.Api.Storage;

public sealed class UserRepository
{
    private readonly IJsonDataStore _store;
    private readonly IClock _clock;

    public UserRepository(IJsonDataStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _store.ReadAsync(doc => doc.Users.FirstOrDefault(u => u.Id == id), cancellationToken);

    public Task<User?> FindByEmailOrUsernameAsync(string identifier, CancellationToken cancellationToken)
    {
        var value = (identifier ?? "").Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.FromResult<User?>(null);
        }

        return _store.ReadAsync(
            doc => doc.Users.FirstOrDefault(u =>
                u.Email.Equals(value, StringComparison.OrdinalIgnoreCase) ||
                u.Username.Equals(value, StringComparison.OrdinalIgnoreCase)),
            cancellationToken);
    }

    public Task<(User? User, string? Error)> CreateAsync(
        string username,
        string email,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        var normalizedUsername = (username ?? "").Trim();
        var normalizedEmail = (email ?? "").Trim();

        return _store.MutateAsync(doc =>
        {
            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return ((User?)null, "Username is required.");
            }

            if (string.IsNullOrWhiteSpace(normalizedEmail) || !normalizedEmail.Contains('@'))
            {
                return ((User?)null, "A valid email is required.");
            }

            if (doc.Users.Any(u => u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)))
            {
                return ((User?)null, "Email is already registered.");
            }

            if (doc.Users.Any(u => u.Username.Equals(normalizedUsername, StringComparison.OrdinalIgnoreCase)))
            {
                return ((User?)null, "Username is already taken.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = normalizedUsername,
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                CreatedAt = _clock.UtcNow,
            };

            doc.Users.Add(user);
            return (user, (string?)null);
        }, cancellationToken);
    }
}

