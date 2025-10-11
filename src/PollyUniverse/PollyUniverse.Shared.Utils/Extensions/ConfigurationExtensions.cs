using Microsoft.Extensions.Configuration;

namespace PollyUniverse.Shared.Utils.Extensions;

public static class ConfigurationExtensions
{
    public static string GetOrThrow(this IConfiguration configuration, string key)
    {
        return configuration[key] ?? throw new ArgumentNullException(key);
    }
}
