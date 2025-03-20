using System.Runtime.Caching;

namespace JdCookieWpf.Services;

public static class TokenService
{
    private static readonly ObjectCache Cache = MemoryCache.Default;

    public static void Set<T>(string key, T? value, TimeSpan slidingExpiration)
    {
        if (value == null) return;
        CacheItemPolicy policy = new()
        {
            AbsoluteExpiration = DateTime.UtcNow + slidingExpiration
        };
        Cache.Add(key, value, policy);
    }

    public static T? Get<T>(string key) where T : class
    {
        return Cache[key] as T ;
    }

    public static void Remove(string key)
    {
        Cache.Remove(key);
    }
}