using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.MappingBodyBuilders;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.UserMappings;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public class MappingBuilder
{
    private delegate ITypeMapping? BuildMapping(MappingBuilderContext context);

    private static readonly IReadOnlyCollection<BuildMapping> _builders = new BuildMapping[]
    {
        NullableMappingBuilder.TryBuildMapping,
        DerivedTypeMappingBuilder.TryBuildMapping,
        SpecialTypeMappingBuilder.TryBuildMapping,
        DirectAssignmentMappingBuilder.TryBuildMapping,
        QueryableMappingBuilder.TryBuildMapping,
        DictionaryMappingBuilder.TryBuildMapping,
        EnumerableMappingBuilder.TryBuildMapping,
        ImplicitCastMappingBuilder.TryBuildMapping,
        ParseMappingBuilder.TryBuildMapping,
        CtorMappingBuilder.TryBuildMapping,
        StringToEnumMappingBuilder.TryBuildMapping,
        EnumToStringMappingBuilder.TryBuildMapping,
        EnumMappingBuilder.TryBuildMapping,
        DateTimeToDateOnlyMappingBuilder.TryBuildMapping,
        DateTimeToTimeOnlyMappingBuilder.TryBuildMapping,
        ExplicitCastMappingBuilder.TryBuildMapping,
        ToStringMappingBuilder.TryBuildMapping,
        NewInstanceObjectPropertyMappingBuilder.TryBuildMapping,
    };

    private readonly MappingCollection _mappings;

    public MappingBuilder(MappingCollection mappings)
    {
        _mappings = mappings;
    }

    /// <inheritdoc cref="MappingCollection.CallableUserMappings"/>
    public IReadOnlyCollection<IUserMapping> CallableUserMappings => _mappings.CallableUserMappings;

    /// <inheritdoc cref="MappingBuilderContext.FindMapping"/>
    public ITypeMapping? Find(ITypeSymbol sourceType, ITypeSymbol targetType) => _mappings.Find(sourceType, targetType);

    public ITypeMapping? Build(MappingBuilderContext ctx, bool resultIsReusable)
    {
        if (_mappings.GetIncomplete(ctx.Source, ctx.Target) is { } typeMapping)
            return typeMapping;

        var c = 0;
        foreach (var mappingBuilder in _builders)
        {
            c++;
            if (mappingBuilder(ctx) is not { } mapping)
                continue;

            Console.WriteLine($"Params: {ctx.Parameters.Count}");
            Console.WriteLine($"Used Params: {ctx.UsedParameters.Count}");
            mapping.AddParameters(ctx.UsedParameters);

            if (resultIsReusable)
            {
                _mappings.AddIncomplete(mapping);

                MappingBodyBuilder.BuildBody(mapping, ctx);

                _mappings.PopIncomplete(mapping);
            }

            mapping.AddParameters(ctx.UsedParameters);
            if (resultIsReusable)
            {
                _mappings.Add(mapping);
            }
            else
            {
                _mappings.EnqueueToBuildBody(mapping, ctx);
            }

            return mapping;
        }

        return null;
    }
}
