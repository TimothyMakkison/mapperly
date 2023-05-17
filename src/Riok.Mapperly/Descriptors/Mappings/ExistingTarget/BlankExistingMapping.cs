using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using Riok.Mapperly.Descriptors.Mappings.ExistingTarget;

namespace Riok.Mapperly.Descriptors.MappingBuilders;

/// <summary>
/// Doesn't create a mapping for the current member.
/// </summary>
public class BlankExistingMapping : ExistingTargetMapping
{
    public BlankExistingMapping(ITypeSymbol sourceType, ITypeSymbol targetType)
        : base(sourceType, targetType) { }

    public override IEnumerable<StatementSyntax> Build(TypeMappingBuildContext ctx, ExpressionSyntax target) =>
        throw new NotImplementedException();
}
