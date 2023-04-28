using Microsoft.CodeAnalysis;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Diagnostics;
using Riok.Mapperly.Helpers;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

public static class DerivedTypeMappingBuilder
{
    public static ITypeMapping? TryBuildMapping(MappingBuilderContext ctx)
    {
        if (ctx.Configuration.DerivedTypes.Count == 0)
            return null;

        var derivedTypeMappings = BuildDerivedTypeMappings(ctx);
        return new DerivedTypeMapping(ctx.Source, ctx.Target, derivedTypeMappings);
    }

    private static IReadOnlyDictionary<ITypeSymbol, ITypeMapping> BuildDerivedTypeMappings(MappingBuilderContext ctx)
    {
        var derivedTypeMappings = new Dictionary<ITypeSymbol, ITypeMapping>(SymbolEqualityComparer.Default);

        foreach (var config in ctx.Configuration.DerivedTypes)
        {
            // set reference types non-nullable as they can never be null when type-switching.
            var sourceType = config.SourceType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            if (derivedTypeMappings.ContainsKey(sourceType))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.DerivedSourceTypeDuplicated, sourceType);
                continue;
            }

            if (!sourceType.IsAssignableTo(ctx.Compilation, ctx.Source))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.DerivedSourceTypeIsNotAssignableToParameterType, sourceType, ctx.Source);
                continue;
            }

            var targetType = config.TargetType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            if (!targetType.IsAssignableTo(ctx.Compilation, ctx.Target))
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.DerivedTargetTypeIsNotAssignableToReturnType, targetType, ctx.Target);
                continue;
            }

            var mapping = ctx.FindOrBuildMapping(sourceType, targetType);
            if (mapping == null)
            {
                ctx.ReportDiagnostic(DiagnosticDescriptors.CouldNotCreateMapping, sourceType, targetType);
                continue;
            }

            derivedTypeMappings.Add(sourceType, mapping);
        }

        return derivedTypeMappings;
    }
}
