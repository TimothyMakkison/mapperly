using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;

namespace Riok.Mapperly.Configuration;

public class MapperConfiguration
{
    private readonly MappingConfiguration _defaultConfiguration;
    private readonly AttributeDataAccessor _dataAccessor;

    public MapperConfiguration(Compilation compilation, ISymbol mapperSymbol)
    {
        _dataAccessor = new AttributeDataAccessor(compilation);
        Mapper = _dataAccessor.AccessSingle<MapperAttribute>(mapperSymbol);
        _defaultConfiguration = new MappingConfiguration(
            Mapper.EnumMappingStrategy,
            Mapper.EnumMappingIgnoreCase,
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<MapPropertyAttribute>(),
            Array.Empty<MapDerivedType>()
        );
    }

    public MapperAttribute Mapper { get; }

    public MappingConfiguration ForMethod(IMethodSymbol? method)
    {
        if (method == null)
            return _defaultConfiguration;

        var enumMapping = _dataAccessor.AccessFirstOrDefault<MapEnumAttribute>(method);
        var ignoredSourceProperties = _dataAccessor.Access<MapperIgnoreSourceAttribute>(method).Select(x => x.Source).ToList();
        var ignoredTargetProperties = _dataAccessor
            .Access<MapperIgnoreTargetAttribute>(method)
            .Select(x => x.Target)
            // deprecated MapperIgnoreAttribute, but it is still supported by Mapperly.
#pragma warning disable CS0618
            .Concat(_dataAccessor.Access<MapperIgnoreAttribute>(method).Select(x => x.Target))
#pragma warning restore CS0618
            .ToList();
        var propertyConfigurations = _dataAccessor.Access<MapPropertyAttribute>(method).ToList();
        var derivedTypes = _dataAccessor
            .Access<MapDerivedTypeAttribute, MapDerivedType>(method)
            .Concat(_dataAccessor.Access<MapDerivedTypeAttribute<object, object>, MapDerivedType>(method))
            .ToList();
        return new MappingConfiguration(
            enumMapping?.Strategy ?? _defaultConfiguration.EnumMappingStrategy,
            enumMapping?.IgnoreCase ?? _defaultConfiguration.EnumMappingIgnoreCase,
            ignoredSourceProperties,
            ignoredTargetProperties,
            propertyConfigurations,
            derivedTypes
        );
    }

    private MappingConfiguration Default()
    {
        return new MappingConfiguration(
            Mapper.EnumMappingStrategy,
            Mapper.EnumMappingIgnoreCase,
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<MapPropertyAttribute>(),
            Array.Empty<MapDerivedType>()
        );
    }
}
