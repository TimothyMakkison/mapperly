using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Configuration;

public record MappingConfiguration(
    EnumMappingStrategy EnumMappingStrategy,
    bool EnumMappingIgnoreCase,
    IReadOnlyCollection<string> IgnoredSourceProperties,
    IReadOnlyCollection<string> IgnoredTargetProperties,
    IReadOnlyCollection<MapPropertyAttribute> PropertyConfigurations,
    IReadOnlyCollection<MapDerivedType> DerivedTypes
)
{
    public static MappingConfiguration Descend(MappingConfiguration configuration, string sourcePath, string targetPath)
    {
        var a = configuration.IgnoredSourceProperties.Select(x => Strip(x, sourcePath)).WhereNotNull().ToArray();
        var b = configuration.IgnoredTargetProperties.Select(x => Strip(x, targetPath)).WhereNotNull().ToArray();

        return configuration with
        {
            IgnoredSourceProperties = a,
            IgnoredTargetProperties = b,
            PropertyConfigurations = Array.Empty<MapPropertyAttribute>()
        };
    }

    private static string? Strip(string value, string path)
    {
        var member = $"{path}.";
        if (!value.StartsWith(member))
        {
            return null;
        }
        return value.Substring(member.Length);
    }
}
