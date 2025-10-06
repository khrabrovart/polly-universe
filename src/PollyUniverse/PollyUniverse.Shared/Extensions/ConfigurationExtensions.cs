using Microsoft.Extensions.Configuration;

namespace PollyUniverse.Shared.Extensions;

public static class ConfigurationExtensions
{
    public static string GetOrThrow(this IConfigurationRoot config, string key)
    {
        return config[key] ?? throw new ArgumentNullException(key);
    }
}
