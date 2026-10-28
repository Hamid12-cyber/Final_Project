using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace MotoHM.Api.Shared.Security;

public static class CodeHasher
{
    public static string NewCode() => RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

    public static string Hash(string email, string code, string secret)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{email.Trim().ToLowerInvariant()}|{code.Trim()}|{secret}"));
        return Convert.ToHexString(bytes)[..40];
    }

    public static bool Matches(string email, string code, string secret, string? storedHash)
    {
        if (string.IsNullOrEmpty(storedHash)) return false;
        var actual = Encoding.UTF8.GetBytes(Hash(email, code, secret));
        var expected = Encoding.UTF8.GetBytes(storedHash);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}

public static class CodeAttemptLimiter
{
    private const int MaxFailures = 5;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    private sealed record State(int Failures, DateTime? LockedUntilUtc);

    private static readonly ConcurrentDictionary<string, State> States = new();

    public static bool IsLocked(string key)
    {
        if (States.TryGetValue(key, out var s) && s.LockedUntilUtc is { } until)
        {
            if (until > DateTime.UtcNow) return true;
            States.TryRemove(key, out _);
        }
        return false;
    }

    public static void RegisterFailure(string key)
    {
        States.AddOrUpdate(
            key,
            _ => new State(1, null),
            (_, old) =>
            {
                var failures = old.Failures + 1;
                return failures >= MaxFailures
                    ? new State(failures, DateTime.UtcNow.Add(LockDuration))
                    : new State(failures, null);
            });
    }

    public static void Reset(string key) => States.TryRemove(key, out _);
}