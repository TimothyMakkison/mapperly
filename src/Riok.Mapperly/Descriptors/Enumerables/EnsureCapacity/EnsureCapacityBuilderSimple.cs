using Microsoft.CodeAnalysis.CSharp.Syntax;
using Riok.Mapperly.Descriptors.Mappings;
using static Riok.Mapperly.Emit.SyntaxFactoryHelper;

namespace Riok.Mapperly.Descriptors.Enumerables.EnsureCapacity;

public class EnsureCapacityBuilderSimple : IEnsureCapacityBuilder
{
    private readonly string _targetAccessor;
    private readonly string _sourceAccessor;

    public EnsureCapacityBuilderSimple(string targetAccessor, string sourceAccessor)
    {
        _targetAccessor = targetAccessor;
        _sourceAccessor = sourceAccessor;
    }

    public StatementSyntax BuildEnsureCapacityStatement(TypeMappingBuildContext ctx, ExpressionSyntax target)
    {
        return EnsureCapacityHelper.EnsureCapacityStatement(target, MemberAccess(ctx.Source, _sourceAccessor), MemberAccess(target, _targetAccessor));
    }
}
